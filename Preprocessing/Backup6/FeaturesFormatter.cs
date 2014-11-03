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

        // Hash table to link the ASCII code of each diac class to its target code bitfieldValue
        protected Hashtable targetHashTable = new Hashtable();

        // Length of the final features string
        protected int stringLength;

        // Empty feature string
        public static String emptyFeatureString;

        // Offset of the added feature in raw features case
        protected int offset;

        // Number of added features until now
        protected int addedFeatures;

        // Constant for the bitfield length of character feature
        // from 1610 to 1568 = 43 (1610 - 1568 + 1) ID's
        public const int CHAR_FEATURE_BITFIELD_LEN = 43;

        // Constant for the bitfield length of last character feature
        // from 1618 (0x0652) to 1568 (0x0620) (1610 - 1568 + 1) = 51 ID's
        public const int CHAR_INCLUDING_DIACS_FEATURE_BITFIELD_LEN = 51;

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
                    switch (configManager.targetType)
                    {
                        case "DIAC":
                            wordFeaturesLocal.target = new int[1];
                            wordFeaturesLocal.target[0] = (int)GetTarget(words[i].wordName);
                            break;
                        case "POS":
                            wordFeaturesLocal.target = (int[])GetTarget(words[i]).Clone();
                            break;
                        default:
                            Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: DIAC or POS.", configManager.targetType);
                            break;
                    }
                    

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
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

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
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

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
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "WordOnly":

                            // Fill in word ID features
                            FormatWordIDWordFeatures(words[i], ref wordFeaturesLocal);

                            // Now pad the rest of features string if needed
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "MrfAndPOS":
                            // Format the features according to the type configured (Default, Binar or Bitfield)                            
                            FormatMrfWordFeatures(words[i], ref wordFeaturesLocal);

                            // Fill in the POS bit-field
                            if (words[i].POS_IDs != null)
                            {
                                FormatPOSWordFeatures(words[i], ref wordFeaturesLocal);
                            }

                            // Now pad the rest of features string if needed
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

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
                            // Now pad the rest of features string if needed
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);
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
                // Default bit bitfieldValue is 0 unless found in POS_IDs array
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
                            // The + 1 for bitfield length is included in the definition
                            lastCharFeature = lastCharFeature + ConvertToBitfieldString(wordName.ToCharArray()[wordName.Length - j] % 1568 + 1, CHAR_FEATURE_BITFIELD_LEN);
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


        // Method to extract the DIAC target class of the word
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

        // Method to extract the POS target class of the word
        private int[] GetTarget(Word word)
        {
            /*
            // Form empty bitfield of max. length of int
            double targetsBitfield = 0;

            // Start mask
            double mask = 1;

            // Loop on all POS ID's
            if (word.POS_IDs != null)
            {
                foreach (int POSID in word.POS_IDs)
                {
                    // The mask is 1 at the position of the POSID
                    mask = Math.Pow(2, POSID);

                    // Log the position of each ID as 1
                    targetsBitfield |= mask;
                    
                }// end foreach
            }
            //return ConvertToBitfieldStringMultipleOnes(targetsBitfield, numDiacTargets);
             */
            int[] resultPOSTargets;
            if (word.POS_IDs != null)
            {
                resultPOSTargets = new int[word.POS_IDs.Count()];

                // Add 1 to the POS_IDs so that the targets start from 1 not 0
                for (int i = 0; i < word.POS_IDs.Count(); i++)
                {
                    resultPOSTargets[i] = word.POS_IDs[i] + 1;
                }
            }
            else
            {
                logger.LogError("POS Classes can't be obtained", ErrorCode.WORD_POS_CLASS_NOT_FOUND);
                resultPOSTargets = new int[1];
                resultPOSTargets[0] = (int)TargetCode.DEFAULT;
            }

            return resultPOSTargets;
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

            for (int position = (int)configManager.lastCharFeaturesDepth * (int)FeaturesFormatter.CHAR_FEATURE_BITFIELD_LEN+1; position > 0; position--)
            {
                result = result + "0,";
            }// end for
            return result;

        }// end EmptyFeatureString()

        // Utility to convert a bitfieldValue into comma separated bitfield string
        // The encoded string has ONLY 1 POSTION has 1 and others 0's
        // THE RETURNED String LENGTH WILL BE bitfieldLength + 1, assuming the bitfieldValue starts from 0
        // Currenlty the bit-field is encoded reversly (from left to right, 0 means the most left bitfieldValue, while bitFieldLength = most right bitfieldValue). Example: 8 in 8-bitfield: 0,0,0,0,0,0,0,1
        // This is needed only for Target encoding not normal
        // To reverse, reverse the loop
        /*public static String ConvertTargetToBitfieldString(int bitfieldValue, uint bitfieldLength)
        {
            String result = String.Empty;
            
            // It is intended to loop position <= bitfieldLength not position < bitfieldLength since the bitfieldValue 0 means 1 at the first positio and bitfieldValue bitfieldLength means 1 at the last position, so it must be included in the loop to check if it's equal the bitfieldValue or not
            //for (int position = (int)bitfieldLength; position >= 0; position--)
            for (int position = 0; position <= (int)bitfieldLength; position++)
            {
                if(position == bitfieldValue)
                {
                    result = result + "1,";
                }
                else
                {
                    result = result + "0,";
                }//end if-else
                
            }// end for
            return result;
        }*/

        // Utility to convert a bitfieldValue into comma separated bitfield string
        // The encoded string can have ONLY 1 POSTION has 1 and others 0's, or MULTIPLE ones
        // bitfieldValue can't be zero, so if the value is 0-based consider adding +1 in calling this function
        // Reverse encoding direction--> just like what MATLAB does for RawFeaturesFormat in DCONV_convertRawToBitfield.m
        // Note that: in MATLAB
        // x = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,];
        // when we run find(x == 1) we get 16 48 61 not 2 15 27 as expected, since the most right value is considered the MSB not the LSB
        // That's why we make reverse encoding
        public static String ConvertToBitfieldString(int [] bitfieldValues, uint bitfieldLength)
        {
            String result = String.Empty;
            String bitToAdd;
            // The loop ends from bitfieldLength to consider putting the LSB first in the string at the most left position. To reverse, reverse the loop
            // The loop start @1, depending on that the value(s) passed start from 1
            //for (int position = (int)bitfieldLength; position > 0; position--)
            for (int position = 1; position <= (int)bitfieldLength; position++)
            {
                bitToAdd = "0,";
                // Search if the given position is found in the requested bitfield values
                foreach(int bitfieldValue in bitfieldValues)
                {
                    if (position == bitfieldValue)
                    {
                        bitToAdd = "1,";
                    }
                }// end foreach

                result += bitToAdd;

            }// end for
            return result;
        }

        // Utility to convert a bitfieldValue into comma separated bitfield string
        // The input encoded string is assumed to have ONLY 1 POSTION has 1 and others 0's
        // bitfieldValue can't be zero, so if the value is 0-based consider adding +1 in calling this function
        // Reverse encoding direction--> just like what MATLAB does for RawFeaturesFormat in DCONV_convertRawToBitfield.m
        // Note that: in MATLAB
        // x = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,];
        // when we run find(x == 1) we get 16 48 61 not 2 15 27 as expected, since the most right value is considered the MSB not the LSB
        // That's why we make reverse encoding
        public static String ConvertToBitfieldString(int bitfieldValue, uint bitfieldLength)
        {
            String result = String.Empty;

            // The loop ends from bitfieldLength to consider putting the LSB first in the string at the most left position. To reverse, reverse the loop
            // The loop start @1, depending on that the value(s) passed start from 1
            //for (int position = (int)bitfieldLength; position > 0; position--)
            for (int position = 1; position <= (int)bitfieldLength; position++)
            {
                if (position == bitfieldValue)
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


        // Utility to convert a bitfieldValue into comma separated bitfield string
        // The encoded string has MULTIPLE 1 POSTION has 1 and others 0's
        // Currenlty the bit-field is encoded reversly (from left to right, 0 means the most left bitfieldValue, while bitFieldLength = most right bitfieldValue). Example: 8 in 8-bitfield: 0,0,0,0,0,0,0,1
        // This is needed only for Target encoding not normal
        // To reverse, reverse the mask bitfieldValue to start from 1 and reverse the shift direction in the loop
        /*public static String ConvertToBitfieldStringMultipleOnes(int bitfieldValue, uint bitfieldLength)
        {
            // Initialize the mask
            int mask = (int)Math.Pow(2, bitfieldLength);

            // Initialize the result
            String result = String.Empty;

            // Loop on all bitfieldValue positions and set the encoded string position as "1," or "0,"
            for (int position = 0; position < bitfieldLength; position++ )
            {
                if((bitfieldValue & mask) != 0)
                {
                    result += "1,";
                }
                else
                {
                    result += "0,";
                }

                // Move the mask
                mask = mask >> 1;
                
            }

            return result;
        }*/

        // Utility to convert a bitfieldValue into comma separated bitfield string
        // The input is a bitfield value
        // The encoded string COULD have MULTIPLE 1 POSTION has 1 and others 0's
        // To reverse, reverse the mask bitfieldValue to start from 1 and reverse the shift direction in the loop
        // NOT WORKING: the int size is not enough for bitfield lengths like 62
        /*public static String ConvertToBitfieldString(int bitfieldValue, uint bitfieldLength)
        {
            // Initialize the mask
            // bitfieldLength = 3--> 1 @ 2^(3-1) = 4 = 100
            int mask = (int)Math.Pow(2, bitfieldLength - 1);
            //int mask = 1;

            // Initialize the result
            String result = String.Empty;

            // Loop on all bitfieldValue positions and set the encoded string position as "1," or "0,"
            for (int position = 0; position < bitfieldLength; position++ )
            {
                if((bitfieldValue & mask) != 0)
                {
                    result += "1,";
                }
                else
                {
                    result += "0,";
                }

                // Move the mask
                mask = mask >> 1;
                
            }

            return result;
        }*/

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
        /*protected virtual void PadRestOfFeaturesString(ref WordFeatures wordFeatures)
        {
            // Nothing to be done
        }*/
    }

}
