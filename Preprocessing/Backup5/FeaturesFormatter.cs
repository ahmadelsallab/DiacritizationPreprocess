using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;

namespace Preprocessing
{
    abstract class FeaturesFormatter
    {
        // Reference to configuration manager
        protected ConfigurationManager configManager;

        // Reference to logger
        protected Logger logger;

        // Reference to the array of words to extract features
        protected Word[] words;

        // Features of all words
        public WordFeatures[] wordsFeatures;

        // Hash table to link the ASCII code of each diac class to its target code value
        protected Hashtable targetHashTable = new Hashtable();

        // Length of the final features string
        protected int stringLength;

        // Empty feature string
        public static String emptyFeatureString;

        // Offset of the added feature in raw features case
        protected int offset;

        // Number of added features until now
        protected int addedFeatures;

        // Constant for the bitfield length of last character feature
        public const uint LAST_CHAR_FEATURE_BITFIELD_LEN = 42;

        // Constructor
        public FeaturesFormatter(ConfigurationManager configManager, Logger logger, Word [] words)
        {
            this.logger = logger;
            this.configManager = configManager;            
            this.words = words;
            offset = 0;
            addedFeatures = 0;
            //cntr = 0;

            // Initialize the words features
            wordsFeatures = new WordFeatures[words.Length];

            // Build the hash table
            // The following way doesnt work since the GetValues returns the array not the order it's written but the order of assigned values. To work, rearrange ascii codes and target codes in the same order
            TargetCode[] targetCodes = (TargetCode[])Enum.GetValues(typeof(TargetCode));
            TargetAscii[] targetAscii = (TargetAscii[])Enum.GetValues(typeof(TargetAscii));

            // -1 to remove the DEFAULT case which is not actual diac. class to be added to hash table
            for (int i = 0; i < targetAscii.Length - 1; i++)
            {
                targetHashTable.Add((int)targetAscii[i], targetCodes[i]);
            }

        } // end constructor

        // Destructor
        ~FeaturesFormatter()
        {
            // Free the words features
            for(int i = 0; i < wordsFeatures.Length; i++)
            {
                wordsFeatures[i] = null;
            }
            wordsFeatures = null;


            // Force memory freeing
            GC.Collect();
        }

        // Method to start features extraction.
        public void FormatFeatures()
        {
            logger.LogTrace("Features formatting started...");

            // Initialize the words features
            wordsFeatures = new WordFeatures[words.Length];

            ArrayList wordsFeaturesList = new ArrayList();

            int i = 0;
            try
            {
                // Traverse all words
                for (i = 0; i < words.Length ; i++)
                {
                    WordFeatures wordFeaturesLocal = new WordFeatures();

                    // Extract the target
                    wordFeaturesLocal.target = GetTarget(words[i].wordName);

                    // Extract the last characters features
                    wordFeaturesLocal.lastCharFeatures = GetLastCharFeatures(words[i].wordName);
                    //Console.WriteLine("Last Char Features Obtained of " + i);
                    /*if (wordsFeatures[i].target == TargetCode.DAMMETEN)
                    {
                        int x = 10;
                        cntr++;
                    }*/

                    // Reset features offset for Raw case
                    offset = 0;

                    // Reset number of added features
                    addedFeatures = 0;

                    // Check the required features to be out
                    switch(configManager.outputFeatures)
                    {
                        case "All":

                            // Fill in word ID features
                            FormatWordIDWordFeatures(words[i], ref wordFeaturesLocal);

                            // Format the features according to the type configured (Default, Binar or Bitfield)                            
                            FormatMrfWordFeatures(words[i], ref wordFeaturesLocal);

                            // Fill in the POS bit-field
                            if (words[i].POS_IDs != null)
                            {
                                FormatPOSWordFeatures(words[i], ref wordFeaturesLocal);
                            }

                            // Fill in word ID features--> It's recommended to keep the POS features the last so that
                            // any next features positions are after the string of POS
                            // Ex: Word + POS--> POS needs 61 positions--> if Word ID = 1, then it'd be 63, 2, 4, <PAD: 0 ,0..0>
                            // So, keep POS first--> 2, 4, <PADS: 0,0,..>, 63
                            //FormatWordIDWordFeatures(words[i], ref wordFeaturesLocal);

                            // Now pad the rest of features string if needed
                            PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "POSAndWord":

                            // Fill in word ID features
                            FormatWordIDWordFeatures(words[i], ref wordFeaturesLocal);

                            // Fill in the POS bit-field
                            if (words[i].POS_IDs != null)
                            {
                                FormatPOSWordFeatures(words[i], ref wordFeaturesLocal);
                            }

                            // Fill in word ID features--> It's recommended to keep the POS features the last so that
                            // any next features positions are after the string of POS
                            // Ex: Word + POS--> POS needs 61 positions--> if Word ID = 1, then it'd be 63, 2, 4, <PAD: 0 ,0..0>
                            // So, keep POS first--> 2, 4, <PADS: 0,0,..>, 63

                            //FormatWordIDWordFeatures(words[i], ref wordFeaturesLocal);

                            // Now pad the rest of features string if needed
                            PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "MrfAndWord":

                            // Fill in word ID features
                            FormatWordIDWordFeatures(words[i], ref wordFeaturesLocal);

                            // Format the features according to the type configured (Default, Binar or Bitfield)
                            FormatMrfWordFeatures(words[i], ref wordFeaturesLocal);

                            // Fill in word ID features--> It's recommended to keep the POS features the last so that
                            // any next features positions are after the string of POS
                            // Ex: Word + POS--> POS needs 61 positions--> if Word ID = 1, then it'd be 63, 2, 4, <PAD: 0 ,0..0>
                            // So, keep POS first--> 2, 4, <PADS: 0,0,..>, 63
                            //FormatWordIDWordFeatures(words[i], ref wordFeaturesLocal);

                            // Now pad the rest of features string if needed
                            PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "WordOnly":

                            // Fill in word ID features
                            FormatWordIDWordFeatures(words[i], ref wordFeaturesLocal);

                            // Now pad the rest of features string if needed
                            PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "MrfAndPOS":
                            // Format the features according to the type configured (Default, Binar or Bitfield)                            
                            FormatMrfWordFeatures(words[i], ref wordFeaturesLocal);

                            // Fill in the POS bit-field
                            if (words[i].POS_IDs != null)
                            {
                                FormatPOSWordFeatures(words[i], ref wordFeaturesLocal);
                            }
                            
                            break;
                        case "MrfOnly":
                            // Format the features according to the type configured (Default, Binar or Bitfield)
                            FormatMrfWordFeatures(words[i], ref wordFeaturesLocal);

                            break;

                        case "POSOnly":
                            // Fill in the POS bit-field
                            if (words[i].POS_IDs != null)
                            {
                                FormatPOSWordFeatures(words[i], ref wordFeaturesLocal);
                            }
                            break;

                        default:
                            Console.WriteLine("Incorrect features format configuration. {0} is invalid configuration. Valid configurations are: MrfAndPOS, MrfOnly, POSOnly.", configManager.outputFeatures);
                            throw (new IndexOutOfRangeException());

                    }// end switch



                    // Check length of the formatted wordFeature
                    if (!IsConformantStringLen(wordFeaturesLocal))
                    {
                        // Log error
                        logger.LogError("The expected feature string length is " + stringLength + " while this one length is " + wordFeaturesLocal.features.Length,
                                        ErrorCode.NON_CONFORMANT_FEATURE_STRING);

                        //wordsFeatures[i] = null;

                    }
                    else
                    {
                       /* // Now form the central context word features
                        WordFeatures centralContextWordFeautres = wordFeaturesLocal;
                        FormatMrfWordFeatures(words[i], ref centralContextWordFeautres);
                        wordFeaturesLocal.centralContextWordFeatures = centralContextWordFeautres.features;*/

                        // Now form the central context word features
                        wordFeaturesLocal.centralContextWordFeatures = FormatCentralContextWordFeatures(words[i], wordFeaturesLocal.features);

                        // Add the word to the list
                        wordsFeaturesList.Add(wordFeaturesLocal);

                    }

                        
                    // Reset features offset for Raw case
                    offset = 0;

                    // Reset number of added features
                    addedFeatures = 0;

                }// end for

                wordsFeatures = (WordFeatures[])wordsFeaturesList.ToArray(wordsFeaturesList[0].GetType());
                
                logger.LogTrace("Features formatting done successfuly");
            }
            catch(OutOfMemoryException e)
            {
                logger.LogError("Out of memory at word number " + (i + 1).ToString() + "which is" + words[i].wordName , ErrorCode.OUT_OF_MEMORY);
                Console.WriteLine("Out of memory at word number " + (i + 1).ToString() + "which is" + words[i].wordName);
                throw (e);
            }// end catch
        } // end FormatFeatures
       
        // Method to form the bit-field of the POS features
        protected virtual void FormatPOSWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            String bitToAdd;

            // Traverse all positions to set its bit
            for (int position = 0; position <= Parser.maxIDs.POS_IDs[0]; position++)
            {
                // Default bit value is 0 unless found in POS_IDs array
                bitToAdd = "0";

                // Check if the current position exists in the POS_IDs array            
                for (int i = 0; i < word.POS_IDs.Length; i++)
                {
                    // If exists then make the bit to be added 1
                    if (position == word.POS_IDs[i])
                    {                        
                        bitToAdd = "1";
                        break;
                    }
                }// end foreach

                // Write the final string once
                wordFeatures.features = wordFeatures.features + bitToAdd + ",";

            }// end for
        }// end FormatPOSWordFeatures
        
        // Method to put the features in their format
        protected abstract void FormatMrfWordFeatures(Word word, ref WordFeatures wordFeatures);

        // Method to put the features in their format
        protected abstract void FormatWordIDWordFeatures(Word word, ref WordFeatures wordFeatures);

        // Method to format the central word if different from rest of context words
        protected abstract String FormatCentralContextWordFeatures(Word word, String features);

        //protected abstract void FormatCentralContextWordFeatures(Word word, ref WordFeatures wordFeatures);
        
        // Method to extract the last character(s) features
        protected virtual String GetLastCharFeatures(String wordName)
        {
            if (wordName != null)
            {
                // Counter of number of depth characters
                int i = 0;

                // Position of the character
                int j = 2;

                // The returned feature string
                String lastCharFeature = String.Empty;

                while (i < configManager.lastCharFeaturesDepth)
                {
                    try
                    {
                        // Check if the character falls between 0x0620 and 0x064A=> the characters of Arabic
                        if ((int)wordName.ToCharArray()[wordName.Length - j] >= 1568 &&
                            (int)wordName.ToCharArray()[wordName.Length - j] <= 1610)
                        {
                            lastCharFeature = lastCharFeature + ConvertToBitfieldString((int)wordName.ToCharArray()[wordName.Length - j] % 1568,
                                                                                        LAST_CHAR_FEATURE_BITFIELD_LEN);
                            i++;

                        }
                        else
                        {
                            // Move to next character before the current one
                            j++;
                        }
                    }
                    catch
                    {
                        lastCharFeature = lastCharFeature + EmptyLastCharFeatureString();
                        break;
                    }

                }//end while

                return lastCharFeature;

            }
            else
            {
                return "";
            }
        }


        // Method to extract the target class of the word
        private TargetCode GetTarget(String wordName)
        {
            if ((wordName != "") && (wordName != null) && targetHashTable[(int)wordName.ToCharArray().Last()] != null)
            {
                //if
                return (TargetCode)targetHashTable[(int)wordName.ToCharArray().Last()];
            }
            else
            {
                logger.LogError("Diacritization sign can't be obtained", ErrorCode.WORD_DIAC_CLASS_NOT_FOUND);
                return TargetCode.DEFAULT;
            }
        }// end GetTarget

        // Utility to return empty feature string
        protected String EmptyFeatureString()
        {
            String result = String.Empty;
            
            for (int position = (int)stringLength; position > 0; position--)
            {
                result = result + "0,";
            }// end for
            return result;

        }// end EmptyFeatureString()

        // Utility to return empty last character feature
        protected String EmptyLastCharFeatureString()
        {
            String result = String.Empty;

            for (int position = (int)configManager.lastCharFeaturesDepth * (int)FeaturesFormatter.LAST_CHAR_FEATURE_BITFIELD_LEN+1; position > 0; position--)
            {
                result = result + "0,";
            }// end for
            return result;

        }// end EmptyFeatureString()

        // Utility to convert a value into comma separated bitfield string
        // THE RETURNED String LENGTH WILL BE bitfieldLength + 1, assuming the value starts from 0
        public static String ConvertToBitfieldString(int value, uint bitfieldLength)
        {
            String result = String.Empty;
            
            // It is intended to loop position <= bitfieldLength not position < bitfieldLength since the value 0 means 1 at the first positio and value bitfieldLength means 1 at the last position, so it must be included in the loop to check if it's equal the value or not
            //for (int position = (int)bitfieldLength; position >= 0; position--)
            for (int position = 0; position <= (int)bitfieldLength; position++)
            {
                if(position == value)
                {
                    result = result + "1,";
                }
                else
                {
                    result = result + "0,";
                }//end if-else
                
            }// end for
            return result;
        }

        // Utility to compute the required bitfield length in case of Raw features
        /*public static int ComputeStringLength(ConfigurationManager configManager)
        {
            // Decide which type of formatter to use
            switch (configManager.featuresFormat)
            {
                case "Normal":
                    return NormalFeaturesFormatter.ComputeStringLengthNormal(configManager);
                    //break;
                case "Binary":
                    return BinaryFeaturesFormatter.ComputeStringLengthBinary(configManager);
                    //break;
                case "Bitfield":
                    return BitFieldFeaturesFormatter.ComputeStringLengthBitfield(configManager);
                    //break;
                case "Raw":
                    return RawFeaturesFormatter.ComputeStringLengthRaw(configManager);
                    //break;
                default:
                    Console.WriteLine("Incorrect features format configuration. {0} is invalid configuration. Valid configurations are: Normal, Binary and Bitfield", configManager.featuresFormat);
                    throw (new IndexOutOfRangeException());
            }// end switch
        }*/
        // Method to check if the wordFeature string conforms to the expected string length or not
        protected abstract bool IsConformantStringLen(WordFeatures wordFeature);

        // Method to pad the string if needed
        protected virtual void PadRestOfFeaturesString(ref WordFeatures wordFeatures)
        {
            // Nothing to be done
        }
    }

}
