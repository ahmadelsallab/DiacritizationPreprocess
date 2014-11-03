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

        // Reference to parser
        protected Parser parser;

        // Reference to the array of items to extract features
        protected Item[] items;

        // Features of all items
        public Feature[] features;

        // Hash table to link the ASCII code of each diac class to its target code bitfieldValue
        protected Hashtable targetHashTable = new Hashtable();

        // Hash table to link the ASCII code of each diac class to its target code bitfieldValue
        protected static Hashtable staticTargetHashTable = new Hashtable();

        // Length of the final features string
        protected int stringLength;

        // Empty feature string
        public static String emptyFeatureString;

        // Empty target string
        public static String emptyTargetString;

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

        // Longest word length in charaters, including diacritics
        public const int MAX_WORD_LEN = 26;

        public const int START_OF_ARABIC_CHAR_ASCII_CODE = 1568;
        // Constructor
        public FeaturesFormatter(ConfigurationManager configManager, Parser parser, Logger logger, Item [] items)
        {
            this.logger = logger;
            this.configManager = configManager;            
            this.items = items;
            this.parser = parser;
            offset = 0;
            addedFeatures = 0;
            //cntr = 0;

            // Initialize the items features
            features = new Feature[items.Length];

            // Build the hash table
            // The following way doesnt work since the GetValues returns the array not the order it's written but the order of assigned values. To work, rearrange ascii codes and target codes in the same order



            switch (configManager.targetType)
            {
                case "SYNT_DIAC":
                case "FULL_DIAC":
                    TargetDiacAscii[] targetAscii = (TargetDiacAscii[])Enum.GetValues(typeof(TargetDiacAscii));
                    TargetDiacCode[] targetDiacCodes = (TargetDiacCode[])Enum.GetValues(typeof(TargetDiacCode));
                    // -1 to remove the DEFAULT case which is not actual diac. class to be added to hash table
                    for (int i = 0; i < targetAscii.Length - 1; i++)
                    {
                        targetHashTable.Add((int)targetAscii[i], targetDiacCodes[i]);
                        if (staticTargetHashTable[(int)targetAscii[i]] == null)
                        {
                            staticTargetHashTable.Add((int)targetAscii[i], targetDiacCodes[i]);
                        }
                    }
                    break;
                case "POS":
                    switch (configManager.targetMode)
                    {
                        case "Single":
                            TargetPOSID[] targetPOSID = (TargetPOSID[])Enum.GetValues(typeof(TargetPOSID));
                            TargetPOSCode[] targetPOSCodes = (TargetPOSCode[])Enum.GetValues(typeof(TargetPOSCode));
                            // -1 to remove the DEFAULT case which is not actual diac. class to be added to hash table
                            for (int i = 0; i < targetPOSCodes.Length - 1; i++)
                            {
                                targetHashTable.Add((int)targetPOSID[i], (int)targetPOSCodes[i]);
                            }
                            break;
                        case "Multiple":
                            break;
                        default:
                            Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                            break;
                    }// end switch (configManager.targetMode)    
                    break;
                default:
                    Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: POS or SYNT_DIAC.", configManager.targetType);
                    break;
            }// end switch (configManager.targetType)    




                    

        } // end constructor

        // Destructor
        ~FeaturesFormatter()
        {
            // Free the items features
            for(int i = 0; i < features.Length; i++)
            {
                features[i] = null;
            }
            features = null;


            // Force memory freeing
            GC.Collect();
        }

        // Method to start features extraction.
        public void FormatFeatures()
        {
            logger.LogTrace("Features formatting started...");

            // Initialize the items features
            features = new Feature[items.Length];

            ArrayList wordsFeaturesList = new ArrayList();

            int i = 0;
            try
            {
                // Traverse all items
                for (i = 0; i < items.Length ; i++)
                {
                    Feature wordFeaturesLocal = new Feature();

                    // Extract the target
                    switch (configManager.targetType)
                    {
                        case "SYNT_DIAC":
                            wordFeaturesLocal.target = new int[1];
                            wordFeaturesLocal.target[0] = (int)GetTarget(items[i].itemName);
                            break;
                        case "POS":
                            switch (configManager.targetMode)
                            {
                                case "Single":
                                    wordFeaturesLocal.target = new int[1];
                                    wordFeaturesLocal.target[0] = (int)GetTarget(items[i].POS_IDs);
                                    break;
                                case "Multiple":
                                    wordFeaturesLocal.target = (int[])GetTarget(items[i]).Clone();
                                    break;
                                default:
                                    Console.WriteLine("Incorrect TargetMode configuration. {0} is invalid configuration. Valid configurations are: Single or Multiple.", configManager.targetMode);
                                    break;
                            }// end switch (configManager.targetMode)    
                            
                            break;
                        case "FULL_DIAC":
                            wordFeaturesLocal.target = new int[1];
                            wordFeaturesLocal.target[0] = (int)items[i].target;
                            break;
                        default:
                            Console.WriteLine("Incorrect TargetType configuration. {0} is invalid configuration. Valid configurations are: FULL_DIAC, SYNT_DIAC or POS.", configManager.targetType);
                            break;
                    }
                    
                    // Format the targetString
                    FormatTargetStringFeatures(ref wordFeaturesLocal);                  

                    // Extract the last characters features
                    wordFeaturesLocal.lastCharFeatures = GetLastCharFeatures(items[i].itemName);
                    //Console.WriteLine("Last Char Features Obtained of " + i);
                    /*if (features[i].target == TargetDiacCode.DAMMETEN)
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
                            FormatItemIDFeatures(items[i], ref wordFeaturesLocal);

                            // Format the features according to the type configured (Default, Binar or Bitfield)                            
                            FormatMrfFeatures(items[i], ref wordFeaturesLocal);

                            // Fill in the POS bit-field
                            if (items[i].POS_IDs != null)
                            {
                                FormatPOSFeatures(items[i], ref wordFeaturesLocal);
                            }

                            // Fill in word ID features--> It's recommended to keep the POS features the last so that
                            // any next features positions are after the string of POS
                            // Ex: Word + POS--> POS needs 61 positions--> if Word ID = 1, then it'd be 63, 2, 4, <PAD: 0 ,0..0>
                            // So, keep POS first--> 2, 4, <PADS: 0,0,..>, 63
                            //FormatItemIDFeatures(items[i], ref wordFeaturesLocal);

                            // Now pad the rest of features string if needed
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "POSAndWord":

                            // Fill in word ID features
                            FormatItemIDFeatures(items[i], ref wordFeaturesLocal);

                            // Fill in the POS bit-field
                            if (items[i].POS_IDs != null)
                            {
                                FormatPOSFeatures(items[i], ref wordFeaturesLocal);
                            }

                            // Fill in word ID features--> It's recommended to keep the POS features the last so that
                            // any next features positions are after the string of POS
                            // Ex: Word + POS--> POS needs 61 positions--> if Word ID = 1, then it'd be 63, 2, 4, <PAD: 0 ,0..0>
                            // So, keep POS first--> 2, 4, <PADS: 0,0,..>, 63

                            //FormatItemIDFeatures(items[i], ref wordFeaturesLocal);

                            // Now pad the rest of features string if needed
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "MrfAndWord":

                            // Fill in word ID features
                            FormatItemIDFeatures(items[i], ref wordFeaturesLocal);

                            // Format the features according to the type configured (Default, Binar or Bitfield)
                            FormatMrfFeatures(items[i], ref wordFeaturesLocal);

                            // Fill in word ID features--> It's recommended to keep the POS features the last so that
                            // any next features positions are after the string of POS
                            // Ex: Word + POS--> POS needs 61 positions--> if Word ID = 1, then it'd be 63, 2, 4, <PAD: 0 ,0..0>
                            // So, keep POS first--> 2, 4, <PADS: 0,0,..>, 63
                            //FormatItemIDFeatures(items[i], ref wordFeaturesLocal);

                            // Now pad the rest of features string if needed
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "WordOnly":

                            // Fill in word ID features
                            FormatItemIDFeatures(items[i], ref wordFeaturesLocal);

                            // Now pad the rest of features string if needed
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;

                        case "MrfAndPOS":
                            // Format the features according to the type configured (Default, Binar or Bitfield)                            
                            FormatMrfFeatures(items[i], ref wordFeaturesLocal);

                            // Fill in the POS bit-field
                            if (items[i].POS_IDs != null)
                            {
                                FormatPOSFeatures(items[i], ref wordFeaturesLocal);
                            }

                            // Now pad the rest of features string if needed
                            //PadRestOfFeaturesString(ref wordFeaturesLocal);

                            break;
                        case "MrfOnly":
                            // Format the features according to the type configured (Default, Binar or Bitfield)
                            FormatMrfFeatures(items[i], ref wordFeaturesLocal);

                            break;

                        case "POSOnly":
                            // Fill in the POS bit-field
                            if (items[i].POS_IDs != null)
                            {
                                FormatPOSFeatures(items[i], ref wordFeaturesLocal);
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

                        //features[i] = null;

                    }
                    else
                    {
                       /* // Now form the central context word features
                        WordFeatures centralContextWordFeautres = wordFeaturesLocal;
                        FormatMrfFeatures(items[i], ref centralContextWordFeautres);
                        wordFeaturesLocal.centralContextWordFeatures = centralContextWordFeautres.features;*/

                        // Now form the central context word features
                        wordFeaturesLocal.centralContextWordFeatures = FormatCentralContextFeatures(items[i], wordFeaturesLocal.features);

                        wordFeaturesLocal.originalItem = items[i];
                        // Add the word to the list
                        wordsFeaturesList.Add(wordFeaturesLocal);

                    }

                        
                    // Reset features offset for Raw case
                    offset = 0;

                    // Reset number of added features
                    addedFeatures = 0;

                }// end for

                features = (Feature[])wordsFeaturesList.ToArray(wordsFeaturesList[0].GetType());
                
                logger.LogTrace("Features formatting done successfuly");
            }
            catch(OutOfMemoryException e)
            {
                logger.LogError("Out of memory at word number " + (i + 1).ToString() + "which is" + items[i].itemName , ErrorCode.OUT_OF_MEMORY);
                Console.WriteLine("Out of memory at word number " + (i + 1).ToString() + "which is" + items[i].itemName);
                throw (e);
            }// end catch
        } // end FormatFeatures
       
        // Method to form the bit-field of the POS features
        protected virtual void FormatPOSFeatures(Item word, ref Feature wordFeatures)
        {
            String bitToAdd;

            // Traverse all positions to set its bit
            for (int position = 0; position <= parser.maxIDs.POS_IDs[0]; position++)
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
        }// end FormatPOSFeatures
        
        // Method to put the features in their format
        protected abstract void FormatMrfFeatures(Item word, ref Feature wordFeatures);

        // Method to put the features in their format
        protected abstract void FormatItemIDFeatures(Item item, ref Feature wordFeatures);

        // Method to put the word name feature in its char level encoded format
        protected abstract String FormatWordFeaturesCharLevel(String wordName);

         // Method to format the central word if different from rest of context items
        protected abstract String FormatCentralContextFeatures(Item item, String features);

        //protected abstract void FormatCentralContextFeatures(Item word, ref WordFeatures wordFeatures);
        
        // Method to extract the last character(s) features
        protected virtual String GetLastCharFeatures(String itemName)
        {
            if (itemName != null)
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
                        if ((int)itemName.ToCharArray()[itemName.Length - j] >= 1568 &&
                            (int)itemName.ToCharArray()[itemName.Length - j] <= 1610)
                        {
                            // The + 1 for bitfield length is included in the definition
                            lastCharFeature = lastCharFeature + ConvertToBitfieldString(itemName.ToCharArray()[itemName.Length - j] % 1568 + 1, CHAR_FEATURE_BITFIELD_LEN);
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


        // Method to extract the SYNT_DIAC target class of the word
        private TargetDiacCode GetTarget(String itemName)
        {
            if ((itemName != "") && (itemName != null) && targetHashTable[(int)itemName.ToCharArray().Last()] != null)
            {
                //if
                return (TargetDiacCode)targetHashTable[(int)itemName.ToCharArray().Last()];
            }
            else
            {
                logger.LogError("Diacritization sign can't be obtained", ErrorCode.WORD_DIAC_CLASS_NOT_FOUND);
                return TargetDiacCode.DEFAULT;
            }
        }// end GetTarget

        // Static Method to extract the SYNT_DIAC target class of the word
        public static TargetDiacCode GetSyntDiac(String itemName)
        {
            if ((itemName != "") && (itemName != null) && staticTargetHashTable[(int)itemName.ToCharArray().Last()] != null)
            {
                //if
                return (TargetDiacCode)staticTargetHashTable[(int)itemName.ToCharArray().Last()];
            }
            else
            {
                return TargetDiacCode.DEFAULT;
            }
        }// end GetTarget


        // Method to extract the POS target class of the word for single targets
        private TargetPOSCode GetTarget(int [] POS_IDs)
        {
            // Loop on all POS_IDs
            // If at least 1 ID match a POS Target Code, then this is the target. The condition necessary for 
            // this to work is that no 2 IDs occur together, otherwise the first one in the array will block the other
            // for example, this happens with Prepos and Noun, see DisjointMatrix_POS_Group_1.xls

            if (POS_IDs != null)
            {
                foreach (int POS_ID in POS_IDs)
                {
                    // we make POS_ID+1, because the raw IDs are 0-based (although no 0 ID exists in the dataset)
                    if (targetHashTable[POS_ID + 1] != null)
                    {
                        return (TargetPOSCode)(targetHashTable[POS_ID + 1]);
                    }
                }
            }
            //return TargetPOSCode.DEFAULT;
            return TargetPOSCode.UNKNOWN;
        }
        // Method to extract the POS target class of the word for multiple targets
        private int[] GetTarget(Item word)
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
                resultPOSTargets[0] = (int)TargetDiacCode.DEFAULT;
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
        protected virtual String EmptyLastCharFeatureString()
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
        // The input encoded string is assumed to have ONLY 1 POSTION has 1 and others 0's
        // bitfieldValue can't be zero, so if the value is 0-based consider adding +1 in calling this function
        // Reverse encoding direction--> just like what MATLAB does for RawFeaturesFormat in DCONV_convertRawToBitfield.m
        // Note that: in MATLAB
        // x = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,];
        // when we run find(x == 1) we get 16 48 61 not 2 15 27 as expected, since the most right value is considered the MSB not the LSB
        // That's why we make reverse encoding
        // This function takes care of suppressed targets
        public static String ConvertTargetToBitfieldString(int bitfieldValue, uint bitfieldLength, Hashtable suppressTargetsHashTable)
        {
            String result = String.Empty;

            // The loop ends from bitfieldLength to consider putting the LSB first in the string at the most left position. To reverse, reverse the loop
            // The loop start @1, depending on that the value(s) passed start from 1
            //for (int position = (int)bitfieldLength; position > 0; position--)
            for (int position = 1; position <= (int)bitfieldLength; position++)
            {
                if ((String)suppressTargetsHashTable[position.ToString()] != "Suppress")
                {

                    if (position == bitfieldValue)
                    {
                        result = result + "1,";
                    }
                    else
                    {
                        result = result + "0,";
                    }//end if-else
                }

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
        protected abstract bool IsConformantStringLen(Feature wordFeature);

        // Method to format the targetString
        protected abstract void FormatTargetStringFeatures(ref Feature wordFeatures);

        // Method to pad the string if needed
        /*protected virtual void PadRestOfFeaturesString(ref WordFeatures wordFeatures)
        {
            // Nothing to be done
        }*/


        // Method to format the central word + its context word into the wordFeatures.features string
        protected String ExtractCentralWordContextFeatures(Item item)
        {
            String centralWordContextFeaturesString = String.Empty;
            offset = 0;
            switch (configManager.contextType)
            {
                case "Type 1":
                    // Fill in the BEFORE context items
                    for (int j = configManager.centralWordContextBeforeLength; j > 0; j--)
                    {
                        if (item.prevWord != null)
                        {
                            centralWordContextFeaturesString = FormatWordFeaturesCharLevel(item.prevWord.wordNameWithNoDiac) + centralWordContextFeaturesString;
                        }
                        else
                        {
                            centralWordContextFeaturesString = FormatWordFeaturesCharLevel(null) + centralWordContextFeaturesString;
                        }
                        
                    } // end for BEFORE

                    // Fill in the concerned word
                    centralWordContextFeaturesString = centralWordContextFeaturesString + FormatWordFeaturesCharLevel(item.wordNameWithNoDiac);

                    // Fill in the AFTER context items
                    for (int j = 0; j < configManager.centralWordContextAfterLength; j++)
                    {
                        if (item.nextWord != null)
                        {
                            // The next word is not yet parsed, so wordNameWithNoDiac is not set yet, that's why we operate on itemNameWithProperDiacritics, which was set in FileParse, not WordParse
                            //centralWordContextFeaturesString = centralWordContextFeaturesString + FormatWordFeaturesCharLevel(RawTxtParser.RemoveDiacritics(item.nextWord.itemNameWithProperDiacritics));
                            centralWordContextFeaturesString = centralWordContextFeaturesString + FormatWordFeaturesCharLevel(item.nextWord.wordNameWithNoDiac);
                        }
                        else
                        {
                            centralWordContextFeaturesString = centralWordContextFeaturesString + FormatWordFeaturesCharLevel(null);
                        }
                    }
                    break;
                case "Type 2":// Not supported
                default:
                    Console.WriteLine("Only Type 1 context is supported for Central Word Context features");
                    break;
            }// end switch

            return centralWordContextFeaturesString;
        }

        // Utility to return empty target string
        protected abstract String EmptyTargetString();
    }

}
