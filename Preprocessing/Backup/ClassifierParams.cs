using System;
using System.Collections.Generic;
using System.Text;

namespace Preprocessing
{
    class ClassifierParams
    {
        // Identifier of the classifier
        public String classifierName;

        // Path of the classifier trained net parameters
        public string finalNetFullPath;

        // Path of the Configurations.xml used in prepreocessing of classifier training
        public string configurationFileFullPath;

        // Path of maxIDInfo.txt file resulted from parsing the dataset used to train the classifier
        public string maxIDInfoFileFullPath;
    }
}
