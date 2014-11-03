using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using MLApp;
using System.Text.RegularExpressions;
 //“Mwarray.dll”, located in \toolbox\dotnetbuilder\bin\win64\v2.0
//using MathWorks.MATLAB.NET.Arrays;
//using MathWorks.MATLAB.NET.Utility;

namespace Preprocessing
{
    class OutputFileWriter
    {
        // Reference to the file writer
        private StreamWriter outputFile;

        // Reference to the context features
        private Feature[] contextFeatures;

        // Reference to logger
        private Logger logger;

        // Format of output
        private String outputFileFormat;

        // Reference to ConfigurationsManager
        private ConfigurationManager configManager;

        // Reference to parser
        private Parser parser;

        // Number of actual examples
        public int numExamplesInOutFile;

        //public static int cntr;

        // Instantiate MATLAB Engine Interface through com
        MLApp.MLAppClass matlab;

        // Temp string to hold the assignement statement for mTargets
        String targetAssignementString = "mTargets = [\n";

        // Temp string to hold targetAssignementStringthe assignement statement for mFeatures
        String featuresAssignementString = "mFeatures = [\n";

        // File name in case of output format is MATLAB
        String matlabFileName;

        // Root path of matlab workspace
        String rootDirectory;

        // Mode of writting (Train or Test)
        String mode;

        // Features format configuration: Normal, Raw, Binary or Bitfield
        String featuresFormat;

        // The nBitfieldLength parameter to be encoded in Matlab in raw features case
        int contextBitfieldLength;

        // The vector representing the start boundary of each feature chunk
        int [] chunksLen;

        // The vector representing the start boundary of each bitfield offset to be added to each ID
        int [] offset;

        public OutputFileWriter(ConfigurationManager configManager, Logger logger, Parser parser, String rootDirectory, String mode)
        {
            
            this.logger = logger;
            numExamplesInOutFile = 0;
            this.outputFileFormat = configManager.outputFileFormat;
            this.matlabFileName = configManager.matlabOutFilePath + configManager.directorySeparator + configManager.matlabOutFileName;
            this.outputFile = configManager.outputFile;
            this.rootDirectory = rootDirectory;
            this.mode = mode;
            this.featuresFormat = configManager.featuresFormat;
            chunksLen= RawFeaturesFormatter.ComputeChunksLength(configManager, parser);
            contextBitfieldLength = RawFeaturesFormatter.ComputeBitfieldLengthAndOffset(configManager, out offset, parser);
            this.configManager = configManager;
            this.parser = parser;
        }

        public void WriteOutputFile(Feature[] contextFeatures, OutFileMode mode)
        {
            switch (mode)
            {
                case OutFileMode.START:
                    StartFile(outputFile);
                    break;
                case OutFileMode.APPEND:
                    AppendToFile(contextFeatures);
                    break;
                case OutFileMode.FINISH:
                    FinalizeFile();
                    break;
                default:
                    break;
            }// end switch
        }// end WriteOutputFile

        private void AppendToFile(Feature[] contextFeatures)
        {
            this.contextFeatures = contextFeatures;
            switch (outputFileFormat)
            {
                case "MATLAB":
                    WriteOutputMatlabScript();
                    break;
                case "TxtFile":
                    WriteOutputTxtFile();
                    break;
                case "MatlabWorkspace":
                    WriteOutputMatlabworkspace();
                    break;
                default:
                    Console.WriteLine("Incorrect file format configuration. {0} is invalid configuration. Valid configurations are: MATLAB, MatlabWorkspace or TxtFile", outputFileFormat);
                    throw (new IndexOutOfRangeException());
            }// end switch

        }

        private void StartFile(StreamWriter outputFile)
        {
            String temp;
            switch (outputFileFormat)
            {
                case "MATLAB":
                    // Write the function header
                    this.outputFile = new StreamWriter(File.Open(matlabFileName, FileMode.OpenOrCreate, FileAccess.Write), Encoding.Default);
                    this.outputFile.AutoFlush = true;
                    this.outputFile.WriteLine("% Function:\n% Converts the input txt file into features and targets MATLAB vectors.\n% Inputs:\n% None.\n% Output:\n% mFeatures: Matrix (nxm), where n is the number of examples and m is the features vector length\n% mTargets: Matrix (nxl), where n is the number of examples and l is the number of target classes\nfunction [mFeatures, mTargets] = DCONV_convertMatlabInput()");
                    this.outputFile.WriteLine(@"CONFIG_strParams.nBitfieldLength = " + contextBitfieldLength + ";");
                    this.outputFile.WriteLine(@"CONFIG_strParams.vChunkLength = [");
                    temp = "";
                    for (int i = 0; i < chunksLen.Length; i++)
                    {
                        temp += chunksLen[i].ToString() + ",";
                    }
                    this.outputFile.WriteLine(temp + "];");
                    this.outputFile.WriteLine(@"CONFIG_strParams.vOffset = [");
                    temp = "";
                    for (int i = 0; i < offset.Length; i++)
                    {
                        temp += chunksLen[i].ToString() + ",";
                    }
                    this.outputFile.WriteLine(temp + "];");                    
                    
                    break;

                case "TxtFile":
                    this.outputFile = outputFile;
                    this.outputFile.WriteLine(contextBitfieldLength);
                    temp = "";
                    for (int i = 0; i < chunksLen.Length; i++)
                    {
                        temp += chunksLen[i].ToString() + ",";                        
                    }
                    this.outputFile.WriteLine(temp);
                    temp = "";
                    for (int i = 0; i < offset.Length; i++)
                    {
                        temp += chunksLen[i].ToString() + ",";
                    }
                    this.outputFile.WriteLine(temp);
                    break;

                case "MatlabWorkspace":
                    matlab = new MLApp.MLAppClass();
                    matlab.Execute(@"mTargets = [];");
                    matlab.Execute("filesCount = 0;");
                    matlab.Execute(@"mFeatures = [];");
                    matlab.Execute(@"nBitfieldLength = " + contextBitfieldLength + ";");
                    matlab.PutWorkspaceData("vChunkLength", "base", chunksLen);
                    matlab.PutWorkspaceData("vOffset", "base", offset);                    
                    break;

                default:
                    Console.WriteLine("Incorrect file format configuration. {0} is invalid configuration. Valid configurations are: MATLAB, or TxtFile", outputFileFormat);
                    throw (new IndexOutOfRangeException());
            }// end switch

        }

        private void FinalizeFile()
        {
            String s = String.Empty;
            switch (outputFileFormat)
            {
                case "MATLAB":
                    // Terminate target and features variables
                    targetAssignementString += "];";

                    featuresAssignementString += "];";

                    // Write the target assignement
                    outputFile.WriteLine(targetAssignementString);

                    // Write the features assigenement
                    outputFile.WriteLine(featuresAssignementString);

                    outputFile.WriteLine("end % end function");
                    
                    outputFile.Close();

                    break;
                case "MatlabWorkspace":
                    switch (mode)
                    {
                        case "Train":
                            s = matlab.Execute(@"mTrainTargets = mTargets;");
                            if (Regex.Match(s, "Error").Success)
                            {
                                logger.LogError(s, ErrorCode.MATLAB_ERROR);
                            }
                            s = matlab.Execute(@"mTrainFeatures = mFeatures;");
                            if (Regex.Match(s, "Error").Success)
                            {
                                logger.LogError(s, ErrorCode.MATLAB_ERROR);
                            }
                            s = matlab.Execute(@"clear mTargets mFeatures");
                            if (Regex.Match(s, "Error").Success)
                            {
                                logger.LogError(s, ErrorCode.MATLAB_ERROR);
                            }
                            break;
                        case "Test":
                            s = matlab.Execute(@"mTestTargets = mTargets;");
                            if (Regex.Match(s, "Error").Success)
                            {
                                logger.LogError(s, ErrorCode.MATLAB_ERROR);
                            }
                            s = matlab.Execute(@"mTestFeatures = mFeatures;");
                            if (Regex.Match(s, "Error").Success)
                            {
                                logger.LogError(s, ErrorCode.MATLAB_ERROR);
                            }
                            s = matlab.Execute(@"clear mTargets mFeatures");
                            if (Regex.Match(s, "Error").Success)
                            {
                                logger.LogError(s, ErrorCode.MATLAB_ERROR);
                            }
                            break;
                    }// end switch
                    s = matlab.Execute(@"mTrainTargets = mTargets;");
                    if (Regex.Match(s, "Error").Success)
                    {
                        logger.LogError(s, ErrorCode.MATLAB_ERROR);
                    }
                    s = matlab.Execute(@"save('" + rootDirectory + @"\input_data', '-v7.3');");
                    if (Regex.Match(s, "Error").Success)
                    {
                        logger.LogError(s, ErrorCode.MATLAB_ERROR);
                    }
                    break;

            }// end switch 
        }


        private void WriteOutputTxtFile()
        {
            logger.LogTrace("Writting to output file...");

            // Get number of targets
            /*uint numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1 - (uint)configManager.suppressTargetsHashTable.Count;// -1 remove DEFAULT and remove suppressed targets
            uint numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1 - (uint)configManager.suppressTargetsHashTable.Count;*/
            uint numDiacTargets;
            uint numPOSTargets;

            
            foreach (Feature contextFeature in contextFeatures)
            {
                // Write to the output file
                if (configManager.logExamplesEvenNoTargetDetected && ((String)configManager.suppressTargetsHashTable[contextFeature.target[0].ToString()] != "Suppress"))
                {
                    // DEFAULT classes are written
                    if (contextFeature.features != null)
                    {
                        String targetString = String.Empty;

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;// -1 remove DEFAULT
                                //targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numDiacTargets);
                                targetString = FeaturesFormatter.ConvertTargetToBitfieldString(contextFeature.target[0], numDiacTargets, configManager.suppressTargetsHashTable);
                                break;
                            case "POS":

                                switch (configManager.targetMode)
                                {
                                    case "Single":
                                        numPOSTargets = (uint)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length - 1;// -1 remove DEFAULT
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numPOSTargets);

                                        break;
                                    case "Multiple":
                                        numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1;
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numPOSTargets);
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                        break;
                                }// end switch (configManager.targetMode)    
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch                      

                        //String targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numDiacTargets);

                        // Write the target
                        outputFile.WriteLine(targetString);

                        // Write the features
                        outputFile.WriteLine(contextFeature.features);

                        numExamplesInOutFile++;

                    }

                }
                else
                {
                    // Is it default
                    bool isDefault = true;
                    switch (configManager.targetType)
                    {
                        case "SYNT_DIAC":
                        case "FULL_DIAC":
                        case "ClassifySyntDiac":
                            isDefault = (contextFeature.target[0] == (int)TargetDiacCode.DEFAULT);
                            break;
                        case "POS":
                            isDefault = (contextFeature.target[0] == (int)TargetPOSCode.DEFAULT);
                            break;
                        default:
                            Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                            break;
                    } // end switch

                    // DEFAULT classes are not written
                    if (!isDefault && (contextFeature.features != null)
                         && ((String)configManager.suppressTargetsHashTable[contextFeature.target[0].ToString()] != "Suppress"))
                    {
                        String targetString = String.Empty;

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;// -1 remove DEFAULT
                                //targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numDiacTargets);
                                targetString = FeaturesFormatter.ConvertTargetToBitfieldString(contextFeature.target[0], numDiacTargets, configManager.suppressTargetsHashTable);
                                break;
                            case "POS":

                                switch (configManager.targetMode)
                                {
                                    case "Single":
                                        numPOSTargets = (uint)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length - 1;// -1 remove DEFAULT
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numPOSTargets);

                                        break;
                                    case "Multiple":
                                        numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1;
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numPOSTargets);
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                        break;
                                }// end switch (configManager.targetMode)    
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch                       

                        //String targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numDiacTargets);

                        // Write the target
                        outputFile.WriteLine(targetString);

                        // Write the features
                        outputFile.WriteLine(contextFeature.features);

                        numExamplesInOutFile++;

                    }

                }

            }// end foreach

            


            logger.LogTrace("Writting to output file done successfuly");
        }//end WriteOutputTxtFile

        private void WriteOutputMatlabScript()
        {
            logger.LogTrace("Writting to output file...");


            // Get number of targets
            /*uint numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1 - (uint)configManager.suppressTargetsHashTable.Count;// -1 remove DEFAULT
            uint numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1 - (uint)configManager.suppressTargetsHashTable.Count;*/

            uint numDiacTargets;
            uint numPOSTargets;

            foreach (Feature contextFeature in contextFeatures)
            {
                // Write to the output file
                if (configManager.logExamplesEvenNoTargetDetected && ((String)configManager.suppressTargetsHashTable[contextFeature.target[0].ToString()] != "Suppress"))
                {
                    // DEFAULT classes are written
                    if (contextFeature.features != null)
                    {
                        String targetString = String.Empty;


                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;// -1 remove DEFAULT
                                //targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numDiacTargets);
                                targetString = FeaturesFormatter.ConvertTargetToBitfieldString(contextFeature.target[0], numDiacTargets, configManager.suppressTargetsHashTable);
                                break;
                            case "POS":

                                switch (configManager.targetMode)
                                {
                                    case "Single":
                                        numPOSTargets = (uint)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length - 1;// -1 remove DEFAULT
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numPOSTargets);

                                        break;
                                    case "Multiple":
                                        numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1;
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numPOSTargets);
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                        break;
                                }// end switch (configManager.targetMode)    
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch                       

                        //String targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numDiacTargets);

                        // Write the target
                        targetAssignementString += targetString;

                        // Write the features
                        featuresAssignementString += contextFeature.features;

                        numExamplesInOutFile++;

                    }

                }
                else
                {
                    // Is it default
                    bool isDefault = true;
                    switch (configManager.targetType)
                    {
                        case "SYNT_DIAC":
                        case "FULL_DIAC":
                        case "ClassifySyntDiac":
                            isDefault = (contextFeature.target[0] == (int)TargetDiacCode.DEFAULT);
                            break;
                        case "POS":
                            isDefault = (contextFeature.target[0] == (int)TargetPOSCode.DEFAULT);
                            break;
                        default:
                            Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                            break;
                    } // end switch

                    // DEFAULT classes are not written
                    if (!isDefault && (contextFeature.features != null)
                        && ((String)configManager.suppressTargetsHashTable[contextFeature.target[0].ToString()] != "Suppress"))
                    {
                        String targetString = String.Empty;


                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;// -1 remove DEFAULT
                                //targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numDiacTargets);
                                targetString = FeaturesFormatter.ConvertTargetToBitfieldString(contextFeature.target[0], numDiacTargets, configManager.suppressTargetsHashTable);
                                break;
                            case "POS":

                                switch (configManager.targetMode)
                                {
                                    case "Single":
                                        numPOSTargets = (uint)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length - 1;// -1 remove DEFAULT
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numPOSTargets);

                                        break;
                                    case "Multiple":
                                        numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1;
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numPOSTargets);
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                        break;
                                }// end switch (configManager.targetMode)    
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch                       

                        //String targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numDiacTargets);

                        // Write the target
                        targetAssignementString += targetString;

                        // Write the features
                        featuresAssignementString += contextFeature.features;

                        numExamplesInOutFile++;

                    }
                    
                }
            }// end foreach
            logger.LogTrace("Writting to output file done successfuly");


        }//end WriteOutputMatlabScript

        private void WriteOutputMatlabworkspace()
        {
            logger.LogTrace("Writting to output workspace...");

            matlab = new MLApp.MLAppClass();
            matlab.Execute(@"nBitfieldLength = " + contextBitfieldLength + ";");
            matlab.PutWorkspaceData("vChunkLength", "base", chunksLen);
            matlab.PutWorkspaceData("vOffset", "base", offset);

            // Get number of targets
            /*uint numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1 - (uint)configManager.suppressTargetsHashTable.Count;// -1 remove DEFAULT
            uint numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1 - (uint)configManager.suppressTargetsHashTable.Count;*/
            uint numDiacTargets; 
            uint numPOSTargets; 
            //String[] featuresStringArr = new String[contextFeatures.Length];
            //String[] targetsStringArr = new String[contextFeatures.Length];
            //Double[] s = new Double[contextFeatures.Length];
            ArrayList featuresStringArr = new ArrayList();
            ArrayList targetsStringArr = new ArrayList();


            foreach (Feature contextFeature in contextFeatures)
            {
                //s[i] = Double.Parse(contextFeature.features);
                //featuresStringArr[i] = contextFeature.features;
                //i++;
                // Write to the output file

                // Write to the output file
                if (configManager.logExamplesEvenNoTargetDetected && ((String)configManager.suppressTargetsHashTable[contextFeature.target[0].ToString()] != "Suppress"))
                {
                    // DEFAULT classes are written
                    if (contextFeature.features != null)
                    {

                        String targetString = String.Empty;

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;// -1 remove DEFAULT
                                //targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numDiacTargets);
                                targetString = FeaturesFormatter.ConvertTargetToBitfieldString(contextFeature.target[0], numDiacTargets, configManager.suppressTargetsHashTable);
                                break;
                            case "POS":
                                
                                switch (configManager.targetMode)
                                {
                                    case "Single":
                                        numPOSTargets = (uint)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length - 1;// -1 remove DEFAULT
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numPOSTargets);

                                        break;
                                    case "Multiple":
                                        numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1;
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numPOSTargets);
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                        break;
                                }// end switch (configManager.targetMode)    
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch                       

                        //String targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numDiacTargets);

                        /*if (contextFeature.target == TargetDiacCode.DAMMETEN)
                        {
                            cntr++;
                        }*/
                        targetsStringArr.Add(targetString);

                        //matlab.PutWorkspaceData("mTarget", "base", targetString);
                        //matlab.Execute(@"mTargets = [mTargets; mTarget];");


                        featuresStringArr.Add(contextFeature.features);

                        //matlab.PutWorkspaceData("mFeature", "base", contextFeature.features);
                        //matlab.Execute(@"mFeatures = [mFeatures; mFeature];");

                        numExamplesInOutFile++;

                    }

                }
                else
                {
                    /*if (configManager.suppressTargetsHashTable[contextFeature.target[0].ToString()] == "Suppress")
                    {
                        int x = 10;
                        x++;
                    }*/

                    // Is it default
                    bool isDefault = true;
                    switch(configManager.targetType)
                    {
                        case "SYNT_DIAC":
                        case "FULL_DIAC":
                        case "ClassifySyntDiac":
                            isDefault = (contextFeature.target[0] == (int)TargetDiacCode.DEFAULT);
                            break;
                        case "POS":
                            isDefault = (contextFeature.target[0] == (int)TargetPOSCode.DEFAULT);
                            break;
                        default:
                            Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                            break;
                    } // end switch
                    // DEFAULT classes are not written
                    if (!isDefault && (contextFeature.features != null)
                        && ((String)configManager.suppressTargetsHashTable[contextFeature.target[0].ToString()] != "Suppress"))
                    {
                        
                        String targetString = String.Empty;

                        switch (configManager.targetType)
                        {
                            case "SYNT_DIAC":
                            case "FULL_DIAC":
                            case "ClassifySyntDiac":
                                numDiacTargets = (uint)((TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode))).Length - 1;// -1 remove DEFAULT
                                //targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numDiacTargets);
                                targetString = FeaturesFormatter.ConvertTargetToBitfieldString(contextFeature.target[0], numDiacTargets, configManager.suppressTargetsHashTable);
                                break;
                            case "POS":

                                switch (configManager.targetMode)
                                {
                                    case "Single":
                                        numPOSTargets = (uint)((TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode))).Length - 1;// -1 remove DEFAULT
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target[0], numPOSTargets);

                                        break;
                                    case "Multiple":
                                        numPOSTargets = (uint)parser.maxIDs.POS_IDs[0] + 1;
                                        targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numPOSTargets);
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                        break;
                                }// end switch (configManager.targetMode)    
                                break;
                            default:
                                Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC, ClassifySyntDiac or POS.", configManager.targetType);
                                break;
                        }// end switch                        
                        
                        //String targetString = FeaturesFormatter.ConvertToBitfieldString(contextFeature.target, numDiacTargets);


                        /*if (contextFeature.target == TargetDiacCode.DAMMETEN)
                        {
                            cntr++;
                        }*/
                        targetsStringArr.Add(targetString);

                        //matlab.PutWorkspaceData("mTarget", "base", targetString);
                        //matlab.Execute(@"mTargets = [mTargets; mTarget];");


                        featuresStringArr.Add(contextFeature.features);

                        //matlab.PutWorkspaceData("mFeature", "base", contextFeature.features);
                        //matlab.Execute(@"mFeatures = [mFeatures; mFeature];");

                        numExamplesInOutFile++;

                    }

                }

            }// end foreach
            /*matlab.PutWorkspaceData("mCurrTargets", "base", targetsStringArr.ToArray());
            matlab.Execute("mTargets = [mTargets; mCurrTargets];");
            matlab.PutWorkspaceData("mCurrFeatures", "base", featuresStringArr.ToArray());
            matlab.Execute("mFeatures = [mFeatures; mCurrFeatures];");*/
            //fileNumber++;
            /*if (fileNumber == 101)
            {
                int x = 1;
            }*/
            String s;
            //targetsStringArr[2] = "1,";
            matlab.PutWorkspaceData("mCurrTargets", "base", targetsStringArr.ToArray());
            s = matlab.Execute("x1 = cell2mat(mCurrTargets)");
            if(Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }
            s = matlab.Execute("x2 = str2num(x1)");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }
            s = matlab.Execute("mTargets = [mTargets; x2]");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }
            matlab.PutWorkspaceData("mCurrFeatures", "base", featuresStringArr.ToArray());

            switch (featuresFormat)
            {
                case "Raw":
                    s= matlab.Execute(@"for(i = 1 : size(mCurrFeatures, 1)) x4(i,:) = str2num(mCurrFeatures{i,:}); end");
                    if (Regex.Match(s, "Error").Success)
                    {
                        logger.LogError(s, ErrorCode.MATLAB_ERROR);
                    }
                    break;
                default:
					s = matlab.Execute("x3 = cell2mat(mCurrFeatures)");
                    if (Regex.Match(s, "Error").Success)
                    {
                        logger.LogError(s, ErrorCode.MATLAB_ERROR);
                    }
					s = matlab.Execute("x4 = str2num(x3)");
                    if (Regex.Match(s, "Error").Success)
                    {
                        logger.LogError(s, ErrorCode.MATLAB_ERROR);
                    }

                    break;
            }// end switch
            s = matlab.Execute("clear x1 x2 x3 mCurrTargets mCurrFeatures;");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }

            //matlab.Execute("nBitfieldLength = 0");
            s = matlab.Execute("find(x4 > nBitfieldLength)");
            if ((String.Compare(s, "\nans =\n\n   Empty matrix: 0-by-1\n\n") != 0) &&
                (String.Compare(s, "\nans =\n\n   Empty matrix: 1-by-0\n\n") != 0))
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
                logger.LogError("Raw ID's are " + s + " while bitfield length is " + contextBitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
            }
            s = matlab.Execute("mFeatures = [mFeatures; x4];");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }
            s = matlab.Execute("clear x4;");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }
            //matlab.Execute("if((size(mFeatures, 1) - size(mTargets, 1)) != 0) violatingFile = filesCount; end;");
            s = matlab.Execute("if ((size(mFeatures, 1) - size(mTargets, 1)) ~= 0) violatingFile(filesCount + 1) = filesCount; end;");

            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }

            s = matlab.Execute("size(mFeatures, 1) - size(mTargets, 1)");
            //int x = String.Compare(s, "\nans =\n\n     0\n\n");
            if (String.Compare(s, "\nans =\n\n     0\n\n") != 0)
            {
                logger.LogError("Difference between targets and features sizes", ErrorCode.MATLAB_ERROR);
            }
            
            //matlab.PutCharArray("mFeatures", "base", s);
            //matlab.PutFullMatrix("mFeatures", "base", s, s);
            s = matlab.Execute("filesCount=filesCount+1;");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }

            logger.LogTrace("Writting to workspace done successfuly");
        }//end WriteOutputMatlabworkspace

        public void ClassifyMatlab(ref Feature[] contextFeatures, String finalNetFullPath)
        {
            logger.LogTrace("Sending the encoded features to MATLAB to classify...");

            matlab = new MLApp.MLAppClass();
            matlab.Execute(@"nBitfieldLength = " + contextBitfieldLength + ";");
            matlab.PutWorkspaceData("vChunkLength", "base", chunksLen);
            matlab.PutWorkspaceData("vOffset", "base", offset);

            ArrayList featuresStringArr = new ArrayList();


            foreach (Feature contextFeature in contextFeatures)
            {
              featuresStringArr.Add(contextFeature.features);

            }// end foreach

            matlab.PutWorkspaceData("mCurrFeatures", "base", featuresStringArr.ToArray());

            String s;
            switch (featuresFormat)
            {
                case "Raw":
                    s = matlab.Execute(@"for(i = 1 : size(mCurrFeatures, 1)) x4(i,:) = str2num(mCurrFeatures{i,:}); end");
                    if (Regex.Match(s, "Error").Success)
                    {
                        logger.LogError(s, ErrorCode.MATLAB_ERROR);
                    }
                    break;
                default:
                    s = matlab.Execute("x3 = cell2mat(mCurrFeatures)");
                    if (Regex.Match(s, "Error").Success)
                    {
                        logger.LogError(s, ErrorCode.MATLAB_ERROR);
                    }
                    s = matlab.Execute("x4 = str2num(x3)");
                    if (Regex.Match(s, "Error").Success)
                    {
                        logger.LogError(s, ErrorCode.MATLAB_ERROR);
                    }

                    break;
            }// end switch
            s = matlab.Execute("clear x1 x2 x3 mCurrTargets mCurrFeatures;");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }

            //matlab.Execute("nBitfieldLength = 0");
            s = matlab.Execute("find(x4 > nBitfieldLength)");
            if (String.Compare(s, "\nans =\n\n   Empty matrix: 0-by-1\n\n") != 0)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
                logger.LogError("Raw ID's are " + s + " while bitfield length is " + contextBitfieldLength, ErrorCode.RAW_ID_MORE_THAN_BITFIELD_LEN);
            }

            // Classify x4 = features vector
            // First load the final_net
            s = matlab.Execute("load '" + finalNetFullPath + "';");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }

            // Goto to the generic classifier path
            s = matlab.Execute("cd '" + configManager.matlabOutFilePath + "';");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }
            // Now, do the classification
            s = matlab.Execute("[vTargetOut] = TST_computeUpperLayerTargets(x4, NM_strNetParams, nBitfieldLength, vChunkLength, vOffset, '" + featuresFormat + "')");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }

            // Parse the return
            // Match all ID's
            MatchCollection targetsOut = Regex.Matches(s, @"[0-9]+");

            if (targetsOut.Count != contextFeatures.Length)
            {
                logger.LogError("Incorrect Classification", ErrorCode.INCORRECT_TARGET_CLASSIFICATION);
            }

            // Insert the matches into ArrayList
            for (int i = 0; i < targetsOut.Count; i++)
            {
                contextFeatures[i].target[0] = Int32.Parse(targetsOut[i].Value);
            }
            s = matlab.Execute("clear x4;");
            if (Regex.Match(s, "Error").Success)
            {
                logger.LogError(s, ErrorCode.MATLAB_ERROR);
            }

            logger.LogTrace("Classification in MATLAB is done successfuly");
        }//end ClassifyMatlab

        ~OutputFileWriter()
        {
           
        }
    }
}
