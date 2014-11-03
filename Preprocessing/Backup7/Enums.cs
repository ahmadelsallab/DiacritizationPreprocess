using System;

namespace Preprocessing
{
    // Ascii codes of Diac classes
    // SHADDA + FAT7A are detected separately
    // KASRTEN, DAMMETEN, FAT7TEN are detected as 1
    // Validate on the sentence: جُورْجْ مُهْتَمٌّ أَعْضَاءِ لَجْنَةِ التَّعَاوُن اقْتِصَادِيَّةٍ اعْتِبَارًا  versus test.txt
    // THIS ENUM MUST BE SET IN ASCENDING ORDER TO ALLOW RIGHT HASH TABLE BUILD
    enum TargetAscii
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
    // THE ORDER SHOULD FOLLOW TargetAscii MEMBERS ORDER
    enum TargetCode
    {
        
        FAT7TEN     = 1,// OK
        DAMMETEN    = 2, // OK
        KASRETEN    = 3,// OK
        FAT7A       = 4,// OK
        DAMMA       = 5,// OK
        KASRA       = 6,// OK
        SHADDA      = 7,// OK
        SUKUN       = 8, // OK
        DEFAULT     //= 8, // Considered SUKKUN-->not recommended since this give false outliers in SUKKUN or any other class. Better to skip thos words
        
        /*
        DEFAULT,    //= 1, // Considered SUKKUN-->not recommended since this give false outliers in SUKKUN or any other class. Better to skip thos words
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
    }

    enum OutFileMode
    {
        START,
        APPEND,
        FINISH
    }


}