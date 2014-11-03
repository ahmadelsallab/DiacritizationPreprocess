using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;

namespace Preprocessing
{
    public class ConfigurationManager
    {
        // The root directory of the Dataset (Train)
        public String rootTrainDirectory;

        // The root directory of the Test Dataset
        public String rootTestDirectory;

        // The path to the current tested configuration (it could be null)
        public String configEnvDirectory;

        // The name of the .mrf folder in each category
        public String mrfFolderName;

        // The name of the .pos folder in each category
        public String posFolderName;

        // The name of the .txt folder in each category
        public String txtFolderName;

        // Logger file name. Only in debug mode.
        public StreamWriter logFile;

        // Constant for directory separator
        public String directorySeparator;

        // Constant for file extension separator
        public String fileExtensionSeparator;

        // Mrf file extension
        public String mrfFileExtension;

        // POS file extension
        public String posFileExtension;

        // Txt file extension
        public String txtFileExtension;

        // Flag to indicate if debug mode is on
        public bool debugMode;

        // Flag to indicate if error logging is enabled or not
        public bool errorLogMode;

        // Flag to indicate if logger shall log trace msgs to console or not
        public bool consoleLogMode;

        // Array that contains the configuration of errors suppression
        public int[] errSupressArr;

        // Features format configuration: Normal, Raw, Binary or Bitfield
        public String featuresFormat;

        // Parameter to indicate which features are needed to be out. Possible values: MrfAndPOS, MrfOnly, POSOnly.
        public String outputFeatures;

        // Type 1 (considering before and after fixed num. of words) or Type 2 (considering context length with no constraint on before and after)
        public String contextType;

        // The length of the context features
        public int contextLength;

        // The length of the context features before the word
        public int contextBeforeLength;

        // The length of the context features after the word
        public int contextAfterLength;

        // The full name of the output file
        public StreamWriter outputFile;

        // The configuration file full name
        public String configFullFileName;

        // The format of the output of pre-processing, it could be TxtFile or MATLAB
        public String outputFileFormat;

        // Matlab file name
        public String matlabOutFileName;

        // Path to put the matlab function in
        public String matlabOutFilePath;

        // the number of last characters to be placed as features
        public int lastCharFeaturesDepth;

        // Hash table of which feautres to be suupressed
        public Hashtable suppressFeaturesHashTable = new Hashtable();

        // Hash table of which feautres to be suupressed
        public Hashtable addFeaturesToCentralContextWord = new Hashtable();

        // Flag to indicate if the user wants to remove words diacritics or not
        public bool stanfordRemoveDiacritics;

        // Format of input POS files: RDI, Stanford
        public String trainInputFormat;
        public String testInputFormat;

        // The name of the Stanford Tagger model file
        public String stanfordTaggerModelName;

        // The name of the Stanford Tagger temp out file name
        public String stanfordTaggerOutFileName;

        // The name of the removed diac file
        public String stanfordFileWithoutDiacs;

        // Flag if mapping is needed to hand coded RDI POS
        public bool mapStanfordToRDI;

        // The structure to be parsed
        // "AnyFile": Parse the root until a file (i.e. not directory) is found and parse it
        // "FolderStructure": The files to be parsed are located in known folder, with known extension
        public String trainInputParsingMode;
        public String testInputParsingMode;

        // Flag to indicate if we wish to log the example feature even if no target
        public bool logExamplesEvenNoTargetDetected;

        public ConfigurationManager(String xmlConfigFileName)
        {
            configFullFileName = xmlConfigFileName;
            
            // Open the configuration document
            XmlDocument xmlConfigStruct = new XmlDocument();
            xmlConfigStruct.Load(xmlConfigFileName);

            // Get reader reference
            XmlNodeReader reader = new XmlNodeReader(xmlConfigStruct);

            // Initialize error supress array. Default is error enabled
            errSupressArr = new int[Enum.GetValues(typeof(ErrorCode)).Length];
            for (int i = 0; i < errSupressArr.Length; i++)
            {
                errSupressArr[i] = Logger.NOT_SUPRESS_ERR_CODE;
            }

            // Parse the xml file
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "ConfigurationParameter" || reader.Name == "Configuration")
                    {
                        if (reader.HasAttributes)
                        {
                            // Read attibutes using linked list of attributes
                            while (reader.MoveToNextAttribute())
                            {
                                switch (reader.Name)
                                {
                                    case "LogFile":
                                        logFile = new StreamWriter(File.Open(reader.Value, FileMode.OpenOrCreate, FileAccess.Write), Encoding.Unicode);
                                        logFile.AutoFlush = true;
                                        break;

                                    case "RootTrainDirectory":
                                        rootTrainDirectory = reader.Value;
                                        break;

                                    case "RootTestDirectory":
                                        rootTestDirectory = reader.Value;
                                        break;

                                    case "ConfigurationEnvironmentDirectory":
                                        configEnvDirectory = reader.Value;
                                        break;

                                    case "MrfFolderName":
                                        mrfFolderName = reader.Value;
                                        break;

                                    case "PosFolderName":
                                        posFolderName = reader.Value;
                                        break;

                                    case "TxtFolderName":
                                        txtFolderName = reader.Value;
                                        break;
                                    
                                    case "DirectorySeparator":
                                        directorySeparator = reader.Value;
                                        break;

                                    case "FileExensionSeparator":
                                        fileExtensionSeparator = reader.Value;
                                        break;
                                        
                                    case "MrfFileExtension":
                                        mrfFileExtension = reader.Value;
                                        break;

                                    case "POSFileExtension":
                                        posFileExtension = reader.Value;
                                        break;

                                    case "TxtFileExtension":
                                        txtFileExtension = reader.Value;
                                        break;

                                    case "DebugMode":
                                        debugMode = bool.Parse(reader.Value);
                                        break;

                                    case "ErrorLogMode":
                                        errorLogMode = bool.Parse(reader.Value);
                                        break;

                                    case "ConsoleLogMode":
                                        consoleLogMode = bool.Parse(reader.Value);
                                        break;

                                    case "SuppressErrType":
                                        try
                                        {
                                            errSupressArr[Int32.Parse(reader.Value) - 1] = Logger.SUPRESS_ERR_CODE;
                                        }
                                        catch(IndexOutOfRangeException e)
                                        {
                                            Console.WriteLine("Incorrect configuration parameter. Error type code is out of range. Please provide code less than " + Enum.GetValues(typeof(ErrorCode)).Length);
                                            throw (e);
                                        }                                                                                
                                        
                                        break;

                                    case "FeaturesFormat":
                                        featuresFormat = reader.Value;                                        
                                        break;
                                        
                                    case "MatlabFunctionName":
                                        matlabOutFileName = "DCONV_convertMatlabInput_" + featuresFormat + ".m";
                                        break;

                                    case "MatlabFilePath":
                                        matlabOutFilePath = reader.Value;
                                        break;

                                    case "OutputFeatures":
                                        outputFeatures = reader.Value;
                                        break;

                                    case "OutputFile":
                                        // Either Encoding.Default of leave empty otherwise MATLAB will not understand the lines: extrac characters are added
                                        outputFile = new StreamWriter(File.Open(reader.Value + "_" + featuresFormat + ".txt", FileMode.OpenOrCreate, FileAccess.Write), Encoding.Default);
                                        break;

                                    case "OutputFileFormat":
                                        outputFileFormat = reader.Value;
                                        break;

                                    case "ContextType":// Type 1 (considering before and after fixed num. of words) or Type 2 (considering context length with no constraint on before and after)
                                        contextType = reader.Value;
                                        break;

                                    case "ContextBeforeLength":// Type 1
                                        contextBeforeLength = Int32.Parse(reader.Value);
                                        break;

                                    case "ContextAfterLength": // Type 1
                                        contextAfterLength = Int32.Parse(reader.Value);
                                        break;

                                    case "ContextLength": // Type 2
                                        contextLength = Int32.Parse(reader.Value);
                                        break;

                                    case "LastCharFeaturesDepth":
                                        lastCharFeaturesDepth = Int32.Parse(reader.Value);
                                        break;

                                    case "MrfSuppressFeature":
                                        suppressFeaturesHashTable.Add(reader.Value, "Suppress");
                                        break;

                                    case "AddFeatureToCentralContextWord":
                                        addFeaturesToCentralContextWord.Add(reader.Value, "Add");
                                        break;

                                    case "StanfordRemoveDiacritics":
                                        stanfordRemoveDiacritics = bool.Parse(reader.Value);
                                        break;

                                    case "TrainInputFormat":
                                        trainInputFormat = reader.Value;
                                        break;

                                    case "TestInputFormat":
                                        testInputFormat = reader.Value;
                                        break;

                                    case "StanfordTaggerModelName":
                                        stanfordTaggerModelName = reader.Value;
                                        break;

                                    case "StanfordTaggerOutFileName":
                                        stanfordTaggerOutFileName = reader.Value;
                                        break;

                                    case "StanfordFileWithoutDiacs":
                                        stanfordFileWithoutDiacs = reader.Value;
                                        break;

                                    case "MapStanfordToRDI":
                                        mapStanfordToRDI = bool.Parse(reader.Value);
                                        break;

                                    case "TrainInputParsingMode":
                                        trainInputParsingMode = reader.Value;
                                        break;

                                    case "TestInputParsingMode":
                                        testInputParsingMode = reader.Value;
                                        break;

                                    case "LogExamplesEvenNoTargetDetected":
                                        logExamplesEvenNoTargetDetected = bool.Parse(reader.Value);
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect configuration parameter." + reader.Value + " is invalid value for Configuration Parameter" + reader.Name);
                                        break;

                                }// end switch (reader.Name)
                            }// end while (reader.MoveToNextAttribute())
                        }// end if (reader.HasAttributes)

                        // Get back to root element
                        reader.MoveToElement();

                    }// end if (reader.NodeType == XmlNodeType.Element)
                    else
                    {
                        Console.WriteLine("Incorrect configuration parameter. Expected: ConfigurationParameter, Found: " + reader.Name);
                    }
                }

            }// end of while (reader.Read())

        }

        ~ConfigurationManager()
        {
            // Close files
            //outputFile.Close();
            //logFile.Close();
        }
    }
    
}
