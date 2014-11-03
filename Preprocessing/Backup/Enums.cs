using System;

namespace Preprocessing
{
    // Ascii codes of Diac classes
    // SHADDA + FAT7A are detected separately
    // KASRTEN, DAMMETEN, FAT7TEN are detected as 1
    // Validate on the sentence: جُورْجْ مُهْتَمٌّ أَعْضَاءِ لَجْنَةِ التَّعَاوُن اقْتِصَادِيَّةٍ اعْتِبَارًا  versus test.txt
    // THIS ENUM MUST BE SET IN ASCENDING ORDER TO ALLOW RIGHT HASH TABLE BUILD
    enum TargetDiacAscii
    {
        FAT7TEN     = 0x064B,// OK
        DAMMETEN    = 0x064C, // OK
        KASRETEN    = 0x064D,// OK
        FAT7A       = 0x064E,// OK
        DAMMA       = 0x064F,// OK
        KASRA       = 0x0650,// OK
        SHADDA      = 0x0651,// OK
        SUKUN       = 0x0652, // OK
        DEFAULT     = 0x0652, // Considered SUKKUN

    }

    // Target Code of Diac classes: Bit-field code
    // THIS ENUM MUST BE SET IN ASCENDING ORDER TO ALLOW RIGHT HASH TABLE BUILD
    // THE ORDER SHOULD FOLLOW TargetDiacAscii MEMBERS ORDER
    enum TargetDiacCode
    {
        
        FAT7TEN     = 1,// OK
        DAMMETEN    = 2, // OK
        KASRETEN    = 3,// OK
        FAT7A       = 4,// OK
        DAMMA       = 5,// OK
        KASRA       = 6,// OK
        SHADDA      = 7,// OK
        SUKUN       = 8, // OK
        DEFAULT     //= 8, // Considered SUKKUN-->not recommended since this give false outliers in SUKKUN or any other class. Better to skip thos items
        
        /*
        DEFAULT,    //= 1, // Considered SUKKUN-->not recommended since this give false outliers in SUKKUN or any other class. Better to skip thos items
        SUKUN       = 1,// OK
        SHADDA      = 2, // OK
        KASRA       = 3,// OK
        DAMMA       = 4,// OK
        FAT7A       = 5,// OK
        KASRETEN    = 6,// OK
        DAMMETEN    = 7,// OK
        FAT7TEN     = 8, // OK
         */
        

    }
    // True IDs as obtained from the dataset parsing of RDI
    enum TargetPOSID
    {
        // NA_0 = 1,
        // NullPrefix = 2,
        // Interrog = 3,
        // Conj = 4,
        // Confirm = 5,
         // Group 1: Prepos = 6,
        //Prepos = 1,
        // Interj = 7,
        // Definit = 8,
        // Group 1: Future = 9,
        //Future = 2,
        // Group 1: ParticleNAASSIB = 10,
        //ParticleNAASSIB = 3,
        // Group 1: Present = 11,
        //Present = 4,
        // Group 1: Imperative = 12,
        //Imperative = 5,
        // Active = 13,
        // Passive = 14,
        // Group 1: Noun = 15,
        //Noun = 6,
        // NounInfinit = 16,
        // NounInfinitLike = 17,
        // SubjNoun = 18,
        // ExaggAdj = 19,
        // ObjNoun = 20,
        // TimeLocNoun = 21,
        // NoSARF = 22,
        // Group 1:PrepPronComp = 23,
        //PrepPronComp = 7,
        // RelPro = 24,
        // DemoPro = 25,
        // InterrogArticle = 26,
        // Group 1:JAAZIMA = 27,
        //JAAZIMA = 8,
        // Group 1:CondJAAZIMA = 28,
        //CondJAAZIMA = 9,
        // Group 1:CondNotJAAZIMA = 29,
        //CondNotJAAZIMA = 10,
        // Group 1:LAA = 30,
        //LAA = 11,
        // NA_30 = 31,
        // Group 1:Except = 32,
        //Except = 12,
        // NoSyntaEffect = 33,
        // DZARF = 34,
        // Group 1:ParticleNAASIKH = 35,
        //ParticleNAASIKH = 13,
        // Group 1:VerbNAASIKH = 36,
        //VerbNAASIKH = 14,
        // MASSDARIYYA = 37,
        // Verb = 38,
        // Intransitive = 39,
        // Group 1:Past = 40,
        //Past = 15,
        // PresImperat = 41,
        // Group 2: MAJZ = 42,
        //MAJZ = 42,
        // Plural = 43,
        // Group 2: MARF = 44,
        //MARF = 44,
        // Group 2: MANSS = 45,
        //MANSS = 45,
        // Group 2: MANS_MAJZ = 46,
        //MANS_MAJZ = 46,
        // Group 3: NullSuffix = 47,
        NullSuffix = 47,
        // RelAdj = 48,
        // Femin = 49,
        // Group 3: PossessPro = 50,
        PossessPro = 50,
        // Masc = 51,
        // Single = 52,
        // Binary = 53,
        // Adjunct = 54,
        // NonAdjunct = 55,
        // Group 2: MANSS_MAGR = 56,
        //MANSS_MAGR = 56,
        // Group 2: MAGR = 57,
        //MAGR = 57,
        // Group 3: ObjPossPro = 58,
        ObjPossPro = 58,
        // SubjPro = 59,
        // Group 3: ObjPro = 60,
        ObjPro = 60,
        // NA_60 = 61,
        // NA_61 = 62,
        DEFAULT,

    }

    // Target Code of POS classes: Bit-field code
    // Modify this enum whenever u produce different POS group of features
    // The code is just the position in bit field, so it MUST BE SERIAL (1, 2, 3..)
    enum TargetPOSCode
    {
        // Note that: +1 is added than POSNames. For ex: Noun = 15 while in .POS file = 14,
        // NA_0 = 1,
        // NullPrefix = 2,
        // Interrog = 3,
        // Conj = 4,
        // Confirm = 5,
        // Prepos = 6,
        // Group 1: Prepos = 1,        
        // Interj = 7,
        // Definit = 8,
        // Future = 9,
        // Group 1:Future = 2,
        // ParticleNAASSIB = 10,
        // Group 1:ParticleNAASSIB = 3,
        // Present = 11,
        // Group 1:Present = 4,
        // Imperative = 12,
        // Group 1:Imperative = 5,
        // Active = 13,
        // Passive = 14,
        // Noun = 15,
        // Group 1:Noun = 6,
        // NounInfinit = 16,
        // NounInfinitLike = 17,
        // SubjNoun = 18,
        // ExaggAdj = 19,
        // ObjNoun = 20,
        // TimeLocNoun = 21,
        // NoSARF = 22,
        // PrepPronComp = 23,
        // Group 1:PrepPronComp = 7,
        // RelPro = 24,
        // DemoPro = 25,
        // InterrogArticle = 26,
        // JAAZIMA = 27,
        // Group 1:JAAZIMA = 8,
        // CondJAAZIMA = 28,
        // Group 1:CondJAAZIMA = 9,
        // CondNotJAAZIMA = 29,
        // Group 1:CondNotJAAZIMA = 10,
        // LAA = 30,
        // Group 1:LAA = 11,
        // NA_30 = 31,
        // Except = 32,
        // Group 1:Except = 12,
        // NoSyntaEffect = 33,
        // DZARF = 34,
        // ParticleNAASIKH = 35,
        // Group 1:ParticleNAASIKH = 13,
        // VerbNAASIKH = 36,
        // Group 1:VerbNAASIKH = 14,
        // MASSDARIYYA = 37,
        // Verb = 38,
        // Intransitive = 39,
        // Past = 40,
        // Group 1:Past = 15,
        // PresImperat = 41,
        // Group 2:  MAJZ = 42,
        //MAJZ = 1,
        // Plural = 43,
        // Group 2: MARF = 44,
        //MARF = 2,
        // Group 2:  MANSS = 45,
        //MANSS = 3,
        // Group 2:  MANS_MAJZ = 46,
        //MANS_MAJZ = 4,
        // Group 3: NullSuffix = 47,
        NullSuffix = 1,
        // RelAdj = 48,
        // Femin = 49,
        // Group 3: PossessPro = 50,
        PossessPro = 2,
        // Masc = 51,
        // Single = 52,
        // Binary = 53,
        // Adjunct = 54,
        // NonAdjunct = 55,
        // Group 2:  MANSS_MAGR = 56,
        //MANSS_MAGR = 5,
        // Group 2:  MAGR = 57,
        //MAGR = 6,
        // Group 3: ObjPossPro = 58,
        ObjPossPro = 3,
        // SubjPro = 59,
        // Grup 3: ObjPro = 60,
        ObjPro = 4,
        // NA_60 = 61,
        // NA_61 = 62,
        // Group 1:UNKNOWN = 16,
        // Group 2:UNKNOWN = 7,
        // Group 3:UNKNOWN = 7,
        UNKNOWN = 5,
        DEFAULT,     

    }

    // Defined error codes
    enum ErrorCode
    {
        PEER_POS_FILE_DOESNT_EXIST      = 1,
        NUM_TAGS_DONT_MATCH             = 2,
        WORDS_NAMES_DONT_MATCH          = 3,
        FILE_IS_EMPTY                   = 4,
        WORD_NAME_IS_EMPTY              = 5,
        ARABIZED_WORD                   = 6,
        OUT_OF_MEMORY                   = 7,
        WORD_DIAC_CLASS_NOT_FOUND       = 8,
        NON_CONFORMANT_FEATURE_STRING   = 9,
        NO_RDI_EQUIVALENT               = 10,
        OUT_OF_VOCABULARY_WORD          = 11,
        MATLAB_ERROR                    = 12,
        WORD_POS_CLASS_NOT_FOUND        = 13,
        RAW_ID_MORE_THAN_BITFIELD_LEN   = 14,
        NON_ARABIC_CHAR                 = 15,
        INCORRECT_TARGET_CLASSIFICATION = 16,
        CLASSIFICATION_ERROR            = 17,
        NO_POS_TAGGER_DEFINED           = 18,
    }

    enum OutFileMode
    {
        START,
        APPEND,
        FINISH
    }


}