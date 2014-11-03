using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class ContextFeatures
    {
        // Comma separated bit-field of category
        public String target;

        // References to features of the words constituting the context
        public WordFeatures[] contextWordsFeatures;

        // Position index of the word in the context
        public int positionIndex;

        // Comma separated features vector. 
        public String features;
    }
}
