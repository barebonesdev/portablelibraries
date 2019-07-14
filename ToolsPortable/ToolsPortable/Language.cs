using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public enum RootLanguages
    {
        English, Arabic, Afrikaans, Azeri, Belarusian, Bulgarian, Bosnian, Catalan, Czech, Welsh, Danish,
        German, Divehi, Greek, Spanish, Estonian, Basque, Persian, Finnish, Faroese, French, Galician, Gujarati,
        Hebrew, Hindi, Croatian, Hungarian, Armenian, Indonesian, Icelandic, Italian, Japanese, Georgian,
        Kazakh, Kannada, Korean, Konkani, Kyrgyz, Lithuanian, Latvian, Maori, Macedonian, Mongolian, Marathi, Malay,
        Maltese, Norwegian, Dutch, NorthernSotho, Punjabi, Polish, Portuguese, Quechua, Romanian, Russian, 
        Sanskrit, Sami, Slovak, Slovenian, Albanian, Serbian, Swedish, Kiswahili, Syriac, Tamil, Telugu, 
        Thai, Tswana, Turkish, Tatar, Ukrainian, Urdu, Uzbek, Vietnamese, Xhosa, Chinese, Zulu
    }

    public class Language
    {
        public RootLanguages RootLanguage { get; private set; }

        /// <summary>
        /// A name gotten from CultureInfo.Name, like "en-US"
        /// </summary>
        /// <param name="name"></param>
        public Language(string name)
        {
            //trim out the "-US" part
            int index = name.IndexOf('-');
            if (index != -1)
                name = name.Substring(0, index);

            //make sure it's lowercase
            name = name.ToLower();

            switch (name)
            {
                case "af":
                    RootLanguage = RootLanguages.Afrikaans;
                    break;

                case "ar":
                    RootLanguage = RootLanguages.Arabic;
                    break;

                case "az":
                    RootLanguage = RootLanguages.Azeri;
                    break;

                case "be":
                    RootLanguage = RootLanguages.Belarusian;
                    break;

                case "bg":
                    RootLanguage = RootLanguages.Bulgarian;
                    break;

                case "bs":
                    RootLanguage = RootLanguages.Bosnian;
                    break;

                case "ca":
                    RootLanguage = RootLanguages.Catalan;
                    break;

                case "cs":
                    RootLanguage = RootLanguages.Czech;
                    break;

                case "cy":
                    RootLanguage = RootLanguages.Welsh;
                    break;

                case "da":
                    RootLanguage = RootLanguages.Danish;
                    break;

                case "de":
                    RootLanguage = RootLanguages.German;
                    break;

                case "dv":
                    RootLanguage = RootLanguages.Divehi;
                    break;

                case "el":
                    RootLanguage = RootLanguages.Greek;
                    break;

                case "en":
                    RootLanguage = RootLanguages.English;
                    break;

                case "es":
                    RootLanguage = RootLanguages.Spanish;
                    break;

                case "et":
                    RootLanguage = RootLanguages.Estonian;
                    break;

                case "eu":
                    RootLanguage = RootLanguages.Basque;
                    break;

                case "fa":
                    RootLanguage = RootLanguages.Persian;
                    break;

                case "fi":
                    RootLanguage = RootLanguages.Finnish;
                    break;

                case "fo":
                    RootLanguage = RootLanguages.Faroese;
                    break;

                case "fr":
                    RootLanguage = RootLanguages.French;
                    break;

                case "gl":
                    RootLanguage = RootLanguages.Galician;
                    break;

                case "gu":
                    RootLanguage = RootLanguages.Gujarati;
                    break;

                case "he":
                    RootLanguage = RootLanguages.Hebrew;
                    break;

                case "hi":
                    RootLanguage = RootLanguages.Hindi;
                    break;

                case "hr":
                    RootLanguage = RootLanguages.Croatian;
                    break;

                case "hu":
                    RootLanguage = RootLanguages.Hungarian;
                    break;

                case "hy":
                    RootLanguage = RootLanguages.Armenian;
                    break;

                case "id":
                    RootLanguage = RootLanguages.Indonesian;
                    break;

                case "is":
                    RootLanguage = RootLanguages.Icelandic;
                    break;

                case "it":
                    RootLanguage = RootLanguages.Italian;
                    break;

                case "ja":
                    RootLanguage = RootLanguages.Japanese;
                    break;

                case "ka":
                    RootLanguage = RootLanguages.Georgian;
                    break;

                case "kk":
                    RootLanguage = RootLanguages.Kazakh;
                    break;

                case "kn":
                    RootLanguage = RootLanguages.Kannada;
                    break;

                case "ko":
                    RootLanguage = RootLanguages.Korean;
                    break;

                case "kok":
                    RootLanguage = RootLanguages.Konkani;
                    break;

                case "ky":
                    RootLanguage = RootLanguages.Kyrgyz;
                    break;

                case "lt":
                    RootLanguage = RootLanguages.Lithuanian;
                    break;

                case "lv":
                    RootLanguage = RootLanguages.Latvian;
                    break;

                case "mi":
                    RootLanguage = RootLanguages.Maori;
                    break;

                case "mk":
                    RootLanguage = RootLanguages.Macedonian;
                    break;

                case "mn":
                    RootLanguage = RootLanguages.Mongolian;
                    break;

                case "mr":
                    RootLanguage = RootLanguages.Marathi;
                    break;

                case "ms":
                    RootLanguage = RootLanguages.Malay;
                    break;

                case "mt":
                    RootLanguage = RootLanguages.Maltese;
                    break;

                case "nb":
                case "nn":
                case "no":
                    RootLanguage = RootLanguages.Norwegian;
                    break;

                case "nl":
                    RootLanguage = RootLanguages.Dutch;
                    break;

                case "ns":
                    RootLanguage = RootLanguages.NorthernSotho;
                    break;

                case "pa":
                    RootLanguage = RootLanguages.Punjabi;
                    break;

                case "pl":
                    RootLanguage = RootLanguages.Polish;
                    break;

                case "pt":
                    RootLanguage = RootLanguages.Portuguese;
                    break;

                case "quz":
                    RootLanguage = RootLanguages.Quechua;
                    break;

                case "ro":
                    RootLanguage = RootLanguages.Romanian;
                    break;

                case "ru":
                    RootLanguage = RootLanguages.Russian;
                    break;

                case "sa":
                    RootLanguage = RootLanguages.Sanskrit;
                    break;

                case "se":
                    RootLanguage = RootLanguages.Sami;
                    break;

                case "sk":
                    RootLanguage = RootLanguages.Slovak;
                    break;

                case "sl":
                    RootLanguage = RootLanguages.Slovenian;
                    break;

                case "sma":
                case "smj":
                case "smn":
                case "sms":
                    RootLanguage = RootLanguages.Sami;
                    break;

                case "sq":
                    RootLanguage = RootLanguages.Albanian;
                    break;

                case "sr":
                    RootLanguage = RootLanguages.Serbian;
                    break;

                case "sv":
                    RootLanguage = RootLanguages.Swedish;
                    break;

                case "sw":
                    RootLanguage = RootLanguages.Kiswahili;
                    break;

                case "syr":
                    RootLanguage = RootLanguages.Syriac;
                    break;

                case "ta":
                    RootLanguage = RootLanguages.Tamil;
                    break;

                case "te":
                    RootLanguage = RootLanguages.Telugu;
                    break;

                case "th":
                    RootLanguage = RootLanguages.Thai;
                    break;

                case "tn":
                    RootLanguage = RootLanguages.Tswana;
                    break;

                case "tr":
                    RootLanguage = RootLanguages.Turkish;
                    break;

                case "tt":
                    RootLanguage = RootLanguages.Tatar;
                    break;

                case "uk":
                    RootLanguage = RootLanguages.Ukrainian;
                    break;

                case "ur":
                    RootLanguage = RootLanguages.Urdu;
                    break;

                case "uz":
                    RootLanguage = RootLanguages.Uzbek;
                    break;

                case "vi":
                    RootLanguage = RootLanguages.Vietnamese;
                    break;

                case "xh":
                    RootLanguage = RootLanguages.Xhosa;
                    break;

                case "zh":
                    RootLanguage = RootLanguages.Chinese;
                    break;

                case "zu":
                    RootLanguage = RootLanguages.Zulu;
                    break;

                default:
                    RootLanguage = RootLanguages.English;
                    break;
            }
        }    }
}
