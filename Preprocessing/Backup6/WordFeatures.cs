using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class WordFeatures
    {
        // Code of the target of the end-case diac. of the word
        public int [] target;

        // In case of POS, the target has multiple ones (i.e. multiple targets). The length of bit-field is beyond integer.
        // So, it's directly encoded as string bit-field
        //public String targetString;

        // Comma separated features vector. 
        public String features;

        // Comma separated features vector of the last character(s). 
        public String lastCharFeatures;

        // Comma separated features vector for central word. 
        public String centralContextWordFeatures;
    }
}
