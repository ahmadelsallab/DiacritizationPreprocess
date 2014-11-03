using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class BitFieldFeaturesFormatter : FeaturesFormatter
    {
        // Constructor
        public BitFieldFeaturesFormatter(ConfigurationManager configManager, Logger logger, Parser parser, Word[] words)
            : base(configManager, logger, parser, words)
        {
            switch (configManager.outputFeatures)
            {
                case "MrfAndPOS":
                    stringLength = (parser.maxIDs.mrfType + 1) + (parser.maxIDs.p + 1) + (parser.maxIDs.r + 1) + (parser.maxIDs.f + 1) + (parser.maxIDs.s + 1) + (parser.maxIDs.POS_IDs[0] + 1);                    
                    break;
                case "MrfOnly":
                    stringLength = (parser.maxIDs.mrfType + 1) + (parser.maxIDs.p + 1) + (parser.maxIDs.r + 1) + (parser.maxIDs.f + 1) + (parser.maxIDs.s + 1);                    
                    break;
                case "POSOnly":
                    stringLength = (parser.maxIDs.POS_IDs[0] + 1);                    
                    break;

            }// end switch

            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();

        }


        // Method to put the features in their format
        protected override void FormatMrfWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            wordFeatures.features = ConvertToBitfieldString(word.mrfType, (uint)parser.maxIDs.mrfType) +
                                    ConvertToBitfieldString(word.p, (uint)parser.maxIDs.p) +
                                    ConvertToBitfieldString(word.r, (uint)parser.maxIDs.r) +
                                    ConvertToBitfieldString(word.f, (uint)parser.maxIDs.f) +
                                    ConvertToBitfieldString(word.s, (uint)parser.maxIDs.s);
        }

        // Method to check if the wordFeature string conforms to the expected string length or not
        protected override bool IsConformantStringLen(WordFeatures wordFeature)
        {
            // *2 to account for "," after each number
            return ((stringLength * 2) == wordFeature.features.Length ? true : false);

        }// end IsConformantStringLen
    }
}
