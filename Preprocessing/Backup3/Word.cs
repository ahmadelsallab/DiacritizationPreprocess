using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class Word
    {
        public String wordName;
        public int mrfType;
        public int p;
        public int r;
        public int f;
        public int s;       
        public int[] POS_IDs;
        public double equivalentPOS_ID;

        // Default Constructor
        public Word()
        {
            p = -1;
            r = -1;
            f = -1;
            s = -1;
            mrfType = -1;
            equivalentPOS_ID = -1;
        }

        // Field init constructor
        public Word(String wordName, int mrfType, int p, int r, int f, int s, int[] POS_IDs, int equivalentPOS_ID)
        {
            this.wordName = wordName;
            this.mrfType = mrfType;
            this.p = p;
            this.r = r;
            this.f = f;
            this.s = s;
            this.POS_IDs = POS_IDs;
            this.equivalentPOS_ID = equivalentPOS_ID;
        }
    }

}
