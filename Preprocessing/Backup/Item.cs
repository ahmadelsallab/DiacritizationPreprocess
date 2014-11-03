using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class Item
    {
        public String itemName;
        public String itemNameWithProperDiacritics;// After applying the rule obained from the confirguration: wordOnlyVocabularyScope: AsIs, RemoveSyntacticDiac or NoDiac
        public String wordNameWithNoDiac;// Used to encode the word name without diac as a feature in FULL_DIAC task
        public int target;
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

        // The position of the character in the word = itemNameWithProperDiacritics
        public int charPosition;

        // Reference to the previous item. This is used to get the Central Word Context in case of FULL_DIAC.
        public Item prevWord;


        // Reference to the next item. This is used to get the Central Word Context in case of FULL_DIAC.
        public Item nextWord;
        
        // Default Constructor
        public Item()
        {
            p = -1;
            r = -1;
            f = -1;
            s = -1;
            mrfType = -1;
            equivalentPOS_ID = -1;
            frequency = 1;
            wordLength = 0;
            prevWord = null;
            nextWord = null;
            charPosition = -1;
            wordNameWithNoDiac = "";

        }

        // Copy constructor
        public Item(Item item)
        {
            this.itemName = item.itemName;
            this.mrfType = item.mrfType;
            this.p = item.p;
            this.r = item.r;
            this.f = item.f;
            this.s = item.s;
            this.POS_IDs = item.POS_IDs;
            this.equivalentPOS_ID = item.equivalentPOS_ID;
            this.vocabularyWordID = item.vocabularyWordID;
            this.frequency = item.frequency;
            this.wordLength = item.wordLength;
            this.target = item.target;
            this.itemNameWithProperDiacritics = item.itemNameWithProperDiacritics;
            this.prevWord = item.prevWord;
            this.nextWord = item.nextWord;
            this.charPosition = item.charPosition;
            this.wordNameWithNoDiac = item.wordNameWithNoDiac;

        }
        // Field init constructor
        public Item(String itemName, int mrfType, int p, int r, int f, int s, int[] POS_IDs, double equivalentPOS_ID, int vocabularyWordID)
        {
            this.itemName = itemName;
            this.mrfType = mrfType;
            this.p = p;
            this.r = r;
            this.f = f;
            this.s = s;
            this.POS_IDs = POS_IDs;
            this.equivalentPOS_ID = equivalentPOS_ID;
            this.vocabularyWordID = vocabularyWordID;
            frequency = 1;
            this.wordLength = itemName.Length;
        }

        // Field init constructor
        public Item(String itemName, int target)
        {
            this.itemName = itemName;
            this.target = target;
        }
    }

}
