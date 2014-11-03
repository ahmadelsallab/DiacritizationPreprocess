using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using MLApp;
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
        private WordFeatures[] contextFeatures;

        // Reference to logger
        private Logger logger;

        // Format of output
        private String outputFileFormat;

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

        public OutputFileWriter(ConfigurationManager configManager, Logger logger, String rootDirectory, String mode)
        {
            
            this.logger = logger;
            numExamplesInOutFile = 0;
            this.outputFileFormat = configManager.outputFileFormat;
            this.matlabFileName = configManager.matlabOutFilePath + configManager.directorySeparator + configManager.matlabOutFileName;
            this.outputFile = configManager.outputFile;
            this.rootDirectory = rootDirectory;
            this.mode = mode;
            this.featuresFormat = configManager.featuresFormat;
            chunksLen= RawFeaturesFormatter.ComputeChunksLength(configManager);
            contextBitfieldLength = RawFeaturesFormatter.ComputeBitfieldLengthAndOffset(configManager, out offset);

        }

        public void WriteOutputFile(WordFeatures[] contextFeatures, OutFileMode mode)
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

        private void AppendToFile(WordFeatures[] contextFeatures)
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
                            matlab.Execute(@"mTrainTargets = mTargets;");
                            matlab.Execute(@"mTrainFeatures = mFeatures;");
                            matlab.Execute(@"clear mTargets mFeatures");
                            break;
                        case "Test":
                            matlab.Execute(@"mTestTargets = mTargets;");
                            matlab.Execute(@"mTestFeatures = mFeatures;");
                            matlab.Execute(@"clear mTargets mFeatures");
                            break;
                    }// end switch

                    matlab.Execute(@"save('" + rootDirectory + @"\input_data');");
                    break;

            }// end switch 
        }


        private void WriteOutputTxtFile()
        {
            logger.LogTrace("Writting to output file...");
            // Get number of targets
            uint numTargets = (uint)((TargetCode[])Enum.GetValues(typeof(TargetCode))).Length - 1;// -1 remove DEFAULT
            

            foreach (WordFeatures contextFeature in contextFeatures)
            {
                // Write to the output file
  
                // DEFAULT classes are not written
                if ((contextFeature.target != TargetCode.DEFAULT) && (contextFeature.features != null)) 
                {
                    // Pass numTargets since ConvertToBitfieldString assumes the value can take 0, while this is not the case for target array, so the bit-field length will be the from 0 to the passed argument, so we pass numTargets - 1
                    // Also, this function assumes the passed value starts from 0 while our target starts from 1 so we subtract 1 from target
                    String targetString = FeaturesFormatter.ConvertToBitfieldString((int)contextFeature.target - 1, numTargets - 1);

                    // Write the target
                    outputFile.WriteLine(targetString);

                    // Write the features
                    outputFile.WriteLine(contextFeature.features);

                    numExamplesInOutFile++;                

                }

            }// end foreach

            


            logger.LogTrace("Writting to output file done successfuly");
        }//end WriteOutputTxtFile

        private void WriteOutputMatlabScript()
        {
            logger.LogTrace("Writting to output file...");


            // Get number of targets
            uint numTargets = (uint)((TargetCode[])Enum.GetValues(typeof(TargetCode))).Length - 1;// -1 remove DEFAULT

            foreach (WordFeatures contextFeature in contextFeatures)
            {
                // Write to the output file

                // DEFAULT classes are not written
                if ((contextFeature.target != TargetCode.DEFAULT) && (contextFeature.features != null))
                {
                    // Pass numTargets since ConvertToBitfieldString assumes the value can take 0, while this is not the case for target array, so the bit-field length will be the from 0 to the passed argument, so we pass numTargets - 1
                    // Also, this function assumes the passed value starts from 0 while our target starts from 1 so we subtract 1 from target
                    String targetString = FeaturesFormatter.ConvertToBitfieldString((int)contextFeature.target - 1, numTargets - 1);

                    // Write the target
                    targetAssignementString += targetString;

                    // Write the features
                    featuresAssignementString += contextFeature.features;

                    numExamplesInOutFile++;

                }

            }// end foreach
            logger.LogTrace("Writting to output file done successfuly");


        }//end WriteOutputMatlabScript

        private void WriteOutputMatlabworkspace()
        {
            logger.LogTrace("Writting to output workspace...");
            // Get number of targets
            uint numTargets = (uint)((TargetCode[])Enum.GetValues(typeof(TargetCode))).Length - 1;// -1 remove DEFAULT
            //String[] featuresStringArr = new String[contextFeatures.Length];
            //String[] targetsStringArr = new String[contextFeatures.Length];
            //Double[] s = new Double[contextFeatures.Length];
            ArrayList featuresStringArr = new ArrayList();
            ArrayList targetsStringArr = new ArrayList();


            foreach (WordFeatures contextFeature in contextFeatures)
            {
                //s[i] = Double.Parse(contextFeature.features);
                //featuresStringArr[i] = contextFeature.features;
                //i++;
                // Write to the output file

                // DEFAULT classes are not written
                if ((contextFeature.target != TargetCode.DEFAULT) && (contextFeature.features != null))
                {
                    // Pass numTargets since ConvertToBitfieldString assumes the value can take 0, while this is not the case for target array, so the bit-field length will be the from 0 to the passed argument, so we pass numTargets - 1
                    // Also, this function assumes the passed value starts from 0 while our target starts from 1 so we subtract 1 from target
                    String targetString = FeaturesFormatter.ConvertToBitfieldString((int)contextFeature.target - 1, numTargets - 1);

                    /*if (contextFeature.target == TargetCode.DAMMETEN)
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

            matlab.PutWorkspaceData("mCurrTargets", "base", targetsStringArr.ToArray());
            matlab.Execute("x1 = cell2mat(mCurrTargets)");
            matlab.Execute("x2 = str2num(x1)");
            matlab.Execute("mTargets = [mTargets; x2];");
            matlab.PutWorkspaceData("mCurrFeatures", "base", featuresStringArr.ToArray());

            switch (featuresFormat)
            {
                case "Raw":
                    matlab.Execute(@"for(i = 1 : size(mCurrFeatures, 1)) x4(i,:) = str2num(mCurrFeatures{i,:}); end");
                    break;
                default:
					matlab.Execute("x3 = cell2mat(mCurrFeatures)");
					matlab.Execute("x4 = str2num(x3)");

                    break;
            }// end switch
            matlab.Execute("clear x1 x2 x3 mCurrTargets mCurrFeatures;");
            matlab.Execute("mFeatures = [mFeatures; x4];");
            matlab.Execute("clear x4;");

            //matlab.PutCharArray("mFeatures", "base", s);
            //matlab.PutFullMatrix("mFeatures", "base", s, s);

            logger.LogTrace("Writting to workspace done successfuly");
        }//end WriteOutputTxtFile

        ~OutputFileWriter()
        {
           
        }
    }
}
