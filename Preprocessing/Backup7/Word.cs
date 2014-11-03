using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class Word
    {
        public String wordName;
        public String wordNameWithProperDiacritics;// After applying the rule obained from the confirguration: wordOnlyVocabularyScope: AsIs, RemoveSyntacticDiac or NoDiac
        public int mrfType;
        public int p;
        public int r;
        public int f;
        public int s;       
        public int[] POS_IDs;// For maxID word: [0]: is the bitfield length, [1] is the max. number of entries of POS IDs for single word
        public double equivalentPOS_ID;

        // ID of the word in vocabulary
        public int vocabularyWordID;

        // Frequency of the word
        public int frequency;

        // Length of the word name. Used for maxID parsing.
        public int wordLength;

        // Default Constructor
        public Word()
        {
            p = -1;
            r = -1;
            f = -1;
            s = -1;
            mrfType = -1;
            equivalentPOS_ID = -1;
            frequency = 1;
            wordLength = 0;
        }

        // Field init constructor
        public Word(String wordName, int mrfType, int p, int r, int f, int s, int[] POS_IDs, double equivalentPOS_ID, int vocabularyWordID)
        {
            this.wordName = wordName;
            this.mrfType = mrfType;
            this.p = p;
            this.r = r;
            this.f = f;
            this.s = s;
            this.POS_IDs = POS_IDs;
            this.equivalentPOS_ID = equivalentPOS_ID;
            this.vocabularyWordID = vocabularyWordID;
            frequency = 1;
            this.wordLength = wordName.Length;
        }
    }

}
