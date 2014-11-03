using System;
using System.Collections.Generic;
using System.Text;

namespace Preprocessing
{
    class POSTaggerParams
    {
        // Name for debugging
        public String posTaggerName;

        // "Stanford" or "DNN"
        public string posTaggerType;

        // The name of the Stanford Tagger model file
        public String stanfordTaggerModelName;

        // The name of the Stanford Tagger temp out file name
        public String stanfordTaggerOutFileName;

        // The name of the removed diac file
        public String stanfordFileWithoutDiacs;

        // Flag if mapping is needed to hand coded RDI POS
        public bool stanfordMapStanfordToRDI;

        // Flag to indicate if the user wants to remove items diacritics or not
        public bool stanfordRemoveDiacritics;

        // Path of the POS Tagger trained net parameters
        public string DNN_POSTaggerFinalNetFullPath;

        // Path of the Configurations.xml used in prepreocessing of POS Tagger training
        public string DNN_POSTaggerConfigurationFileFullPath;

        // Path of maxIDInfo.txt file resulted from parsing the dataset used to train the DNN POS Tagger
        public string DNN_POSTaggerMaxIDInfoFileFullPath;
    }
}
