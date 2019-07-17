﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BareHttpUtilityLibrary
{
    /// <summary>
    /// Created with help from my UrlEncodingISO project in my Console folder
    /// </summary>
    public abstract class BareHttpUtility
    {
        private class Utility_ISO_8859_1 : BareHttpUtility
        {
            protected override string urlEncode(int i)
            {
                switch (i)
                {
                    case 0:
                        return "%00";
                    case 1:
                        return "%01";
                    case 2:
                        return "%02";
                    case 3:
                        return "%03";
                    case 4:
                        return "%04";
                    case 5:
                        return "%05";
                    case 6:
                        return "%06";
                    case 7:
                        return "%07";
                    case 8:
                        return "%08";
                    case 9:
                        return "%09";
                    case 10:
                        return "%0a";
                    case 11:
                        return "%0b";
                    case 12:
                        return "%0c";
                    case 13:
                        return "%0d";
                    case 14:
                        return "%0e";
                    case 15:
                        return "%0f";
                    case 16:
                        return "%10";
                    case 17:
                        return "%11";
                    case 18:
                        return "%12";
                    case 19:
                        return "%13";
                    case 20:
                        return "%14";
                    case 21:
                        return "%15";
                    case 22:
                        return "%16";
                    case 23:
                        return "%17";
                    case 24:
                        return "%18";
                    case 25:
                        return "%19";
                    case 26:
                        return "%1a";
                    case 27:
                        return "%1b";
                    case 28:
                        return "%1c";
                    case 29:
                        return "%1d";
                    case 30:
                        return "%1e";
                    case 31:
                        return "%1f";
                    case 32:
                    case 8192:
                    case 8193:
                    case 8194:
                    case 8195:
                    case 8196:
                    case 8197:
                    case 8198:
                        return "+";
                    case 33:
                    case 65281:
                        return "!";
                    case 34:
                    case 698:
                    case 782:
                    case 8220:
                    case 8221:
                    case 8222:
                    case 65282:
                        return "%22";
                    case 35:
                    case 65283:
                        return "%23";
                    case 36:
                    case 65284:
                        return "%24";
                    case 37:
                    case 65285:
                        return "%25";
                    case 38:
                    case 65286:
                        return "%26";
                    case 39:
                    case 697:
                    case 700:
                    case 712:
                    case 8216:
                    case 8217:
                    case 8242:
                    case 65287:
                        return "%27";
                    case 40:
                    case 65288:
                        return "(";
                    case 41:
                    case 65289:
                        return ")";
                    case 42:
                    case 65290:
                        return "*";
                    case 43:
                    case 65291:
                        return "%2b";
                    case 44:
                    case 8218:
                    case 65292:
                        return "%2c";
                    case 45:
                    case 8208:
                    case 8209:
                    case 8211:
                    case 8212:
                    case 65293:
                        return "-";
                    case 46:
                    case 8226:
                    case 8230:
                    case 65294:
                        return ".";
                    case 47:
                    case 65295:
                        return "%2f";
                    case 48:
                    case 65296:
                        return "0";
                    case 49:
                    case 65297:
                        return "1";
                    case 50:
                    case 65298:
                        return "2";
                    case 51:
                    case 65299:
                        return "3";
                    case 52:
                    case 65300:
                        return "4";
                    case 53:
                    case 65301:
                        return "5";
                    case 54:
                    case 65302:
                        return "6";
                    case 55:
                    case 65303:
                        return "7";
                    case 56:
                    case 65304:
                        return "8";
                    case 57:
                    case 65305:
                        return "9";
                    case 58:
                    case 65306:
                        return "%3a";
                    case 59:
                    case 65307:
                        return "%3b";
                    case 60:
                    case 8249:
                    case 65308:
                        return "%3c";
                    case 61:
                    case 65309:
                        return "%3d";
                    case 62:
                    case 8250:
                    case 65310:
                        return "%3e";
                    case 64:
                    case 65312:
                        return "%40";
                    case 65:
                    case 256:
                    case 258:
                    case 260:
                    case 461:
                    case 478:
                    case 65313:
                        return "A";
                    case 66:
                    case 65314:
                        return "B";
                    case 67:
                    case 262:
                    case 264:
                    case 266:
                    case 268:
                    case 65315:
                        return "C";
                    case 68:
                    case 270:
                    case 272:
                    case 393:
                    case 65316:
                        return "D";
                    case 69:
                    case 274:
                    case 276:
                    case 278:
                    case 280:
                    case 282:
                    case 65317:
                        return "E";
                    case 70:
                    case 401:
                    case 65318:
                        return "F";
                    case 71:
                    case 284:
                    case 286:
                    case 288:
                    case 290:
                    case 484:
                    case 486:
                    case 65319:
                        return "G";
                    case 72:
                    case 292:
                    case 294:
                    case 65320:
                        return "H";
                    case 73:
                    case 296:
                    case 298:
                    case 300:
                    case 302:
                    case 304:
                    case 407:
                    case 463:
                    case 65321:
                        return "I";
                    case 74:
                    case 308:
                    case 65322:
                        return "J";
                    case 75:
                    case 310:
                    case 488:
                    case 65323:
                        return "K";
                    case 76:
                    case 313:
                    case 315:
                    case 317:
                    case 321:
                    case 65324:
                        return "L";
                    case 77:
                    case 65325:
                        return "M";
                    case 78:
                    case 323:
                    case 325:
                    case 327:
                    case 65326:
                        return "N";
                    case 79:
                    case 332:
                    case 334:
                    case 336:
                    case 338:
                    case 415:
                    case 416:
                    case 465:
                    case 490:
                    case 492:
                    case 65327:
                        return "O";
                    case 80:
                    case 65328:
                        return "P";
                    case 81:
                    case 65329:
                        return "Q";
                    case 82:
                    case 340:
                    case 342:
                    case 344:
                    case 65330:
                        return "R";
                    case 83:
                    case 346:
                    case 348:
                    case 350:
                    case 352:
                    case 65331:
                        return "S";
                    case 84:
                    case 354:
                    case 356:
                    case 358:
                    case 430:
                    case 8482:
                    case 65332:
                        return "T";
                    case 85:
                    case 360:
                    case 362:
                    case 364:
                    case 366:
                    case 368:
                    case 370:
                    case 431:
                    case 467:
                    case 469:
                    case 471:
                    case 473:
                    case 475:
                    case 65333:
                        return "U";
                    case 86:
                    case 65334:
                        return "V";
                    case 87:
                    case 372:
                    case 65335:
                        return "W";
                    case 88:
                    case 65336:
                        return "X";
                    case 89:
                    case 374:
                    case 376:
                    case 65337:
                        return "Y";
                    case 90:
                    case 377:
                    case 379:
                    case 381:
                    case 65338:
                        return "Z";
                    case 91:
                    case 65339:
                        return "%5b";
                    case 92:
                    case 65340:
                        return "%5c";
                    case 93:
                    case 65341:
                        return "%5d";
                    case 94:
                    case 708:
                    case 710:
                    case 770:
                    case 65342:
                        return "%5e";
                    case 95:
                    case 717:
                    case 817:
                    case 818:
                    case 65343:
                        return "_";
                    case 96:
                    case 715:
                    case 768:
                    case 8245:
                    case 65344:
                        return "%60";
                    case 97:
                    case 257:
                    case 259:
                    case 261:
                    case 462:
                    case 479:
                    case 65345:
                        return "a";
                    case 98:
                    case 384:
                    case 65346:
                        return "b";
                    case 99:
                    case 263:
                    case 265:
                    case 267:
                    case 269:
                    case 65347:
                        return "c";
                    case 100:
                    case 271:
                    case 273:
                    case 65348:
                        return "d";
                    case 101:
                    case 275:
                    case 277:
                    case 279:
                    case 281:
                    case 283:
                    case 65349:
                        return "e";
                    case 102:
                    case 402:
                    case 65350:
                        return "f";
                    case 103:
                    case 285:
                    case 287:
                    case 289:
                    case 291:
                    case 485:
                    case 487:
                    case 609:
                    case 65351:
                        return "g";
                    case 104:
                    case 293:
                    case 295:
                    case 65352:
                        return "h";
                    case 105:
                    case 297:
                    case 299:
                    case 301:
                    case 303:
                    case 305:
                    case 464:
                    case 65353:
                        return "i";
                    case 106:
                    case 309:
                    case 496:
                    case 65354:
                        return "j";
                    case 107:
                    case 311:
                    case 489:
                    case 65355:
                        return "k";
                    case 108:
                    case 314:
                    case 316:
                    case 318:
                    case 322:
                    case 410:
                    case 65356:
                        return "l";
                    case 109:
                    case 65357:
                        return "m";
                    case 110:
                    case 324:
                    case 326:
                    case 328:
                    case 65358:
                        return "n";
                    case 111:
                    case 333:
                    case 335:
                    case 337:
                    case 339:
                    case 417:
                    case 466:
                    case 491:
                    case 493:
                    case 65359:
                        return "o";
                    case 112:
                    case 65360:
                        return "p";
                    case 113:
                    case 65361:
                        return "q";
                    case 114:
                    case 341:
                    case 343:
                    case 345:
                    case 65362:
                        return "r";
                    case 115:
                    case 347:
                    case 349:
                    case 351:
                    case 353:
                    case 65363:
                        return "s";
                    case 116:
                    case 355:
                    case 357:
                    case 359:
                    case 427:
                    case 65364:
                        return "t";
                    case 117:
                    case 361:
                    case 363:
                    case 365:
                    case 367:
                    case 369:
                    case 371:
                    case 432:
                    case 468:
                    case 470:
                    case 472:
                    case 474:
                    case 476:
                    case 65365:
                        return "u";
                    case 118:
                    case 65366:
                        return "v";
                    case 119:
                    case 373:
                    case 65367:
                        return "w";
                    case 120:
                    case 65368:
                        return "x";
                    case 121:
                    case 375:
                    case 65369:
                        return "y";
                    case 122:
                    case 378:
                    case 380:
                    case 382:
                    case 438:
                    case 65370:
                        return "z";
                    case 123:
                    case 65371:
                        return "%7b";
                    case 124:
                    case 65372:
                        return "%7c";
                    case 125:
                    case 65373:
                        return "%7d";
                    case 126:
                    case 732:
                    case 771:
                    case 65374:
                        return "%7e";
                    case 127:
                        return "%7f";
                    case 128:
                        return "%80";
                    case 129:
                        return "%81";
                    case 130:
                        return "%82";
                    case 131:
                        return "%83";
                    case 132:
                        return "%84";
                    case 133:
                        return "%85";
                    case 134:
                        return "%86";
                    case 135:
                        return "%87";
                    case 136:
                        return "%88";
                    case 137:
                        return "%89";
                    case 138:
                        return "%8a";
                    case 139:
                        return "%8b";
                    case 140:
                        return "%8c";
                    case 141:
                        return "%8d";
                    case 142:
                        return "%8e";
                    case 143:
                        return "%8f";
                    case 144:
                        return "%90";
                    case 145:
                        return "%91";
                    case 146:
                        return "%92";
                    case 147:
                        return "%93";
                    case 148:
                        return "%94";
                    case 149:
                        return "%95";
                    case 150:
                        return "%96";
                    case 151:
                        return "%97";
                    case 152:
                        return "%98";
                    case 153:
                        return "%99";
                    case 154:
                        return "%9a";
                    case 155:
                        return "%9b";
                    case 156:
                        return "%9c";
                    case 157:
                        return "%9d";
                    case 158:
                        return "%9e";
                    case 159:
                        return "%9f";
                    case 160:
                        return "%a0";
                    case 161:
                        return "%a1";
                    case 162:
                        return "%a2";
                    case 163:
                        return "%a3";
                    case 164:
                        return "%a4";
                    case 165:
                        return "%a5";
                    case 166:
                        return "%a6";
                    case 167:
                        return "%a7";
                    case 168:
                        return "%a8";
                    case 169:
                        return "%a9";
                    case 170:
                        return "%aa";
                    case 171:
                        return "%ab";
                    case 172:
                        return "%ac";
                    case 173:
                        return "%ad";
                    case 174:
                        return "%ae";
                    case 175:
                        return "%af";
                    case 176:
                        return "%b0";
                    case 177:
                        return "%b1";
                    case 178:
                        return "%b2";
                    case 179:
                        return "%b3";
                    case 180:
                        return "%b4";
                    case 181:
                        return "%b5";
                    case 182:
                        return "%b6";
                    case 183:
                        return "%b7";
                    case 184:
                        return "%b8";
                    case 185:
                        return "%b9";
                    case 186:
                        return "%ba";
                    case 187:
                        return "%bb";
                    case 188:
                        return "%bc";
                    case 189:
                        return "%bd";
                    case 190:
                        return "%be";
                    case 191:
                        return "%bf";
                    case 192:
                        return "%c0";
                    case 193:
                        return "%c1";
                    case 194:
                        return "%c2";
                    case 195:
                        return "%c3";
                    case 196:
                        return "%c4";
                    case 197:
                        return "%c5";
                    case 198:
                        return "%c6";
                    case 199:
                        return "%c7";
                    case 200:
                        return "%c8";
                    case 201:
                        return "%c9";
                    case 202:
                        return "%ca";
                    case 203:
                        return "%cb";
                    case 204:
                        return "%cc";
                    case 205:
                        return "%cd";
                    case 206:
                        return "%ce";
                    case 207:
                        return "%cf";
                    case 208:
                        return "%d0";
                    case 209:
                        return "%d1";
                    case 210:
                        return "%d2";
                    case 211:
                        return "%d3";
                    case 212:
                        return "%d4";
                    case 213:
                        return "%d5";
                    case 214:
                        return "%d6";
                    case 215:
                        return "%d7";
                    case 216:
                        return "%d8";
                    case 217:
                        return "%d9";
                    case 218:
                        return "%da";
                    case 219:
                        return "%db";
                    case 220:
                        return "%dc";
                    case 221:
                        return "%dd";
                    case 222:
                        return "%de";
                    case 223:
                        return "%df";
                    case 224:
                        return "%e0";
                    case 225:
                        return "%e1";
                    case 226:
                        return "%e2";
                    case 227:
                        return "%e3";
                    case 228:
                        return "%e4";
                    case 229:
                        return "%e5";
                    case 230:
                        return "%e6";
                    case 231:
                        return "%e7";
                    case 232:
                        return "%e8";
                    case 233:
                        return "%e9";
                    case 234:
                        return "%ea";
                    case 235:
                        return "%eb";
                    case 236:
                        return "%ec";
                    case 237:
                        return "%ed";
                    case 238:
                        return "%ee";
                    case 239:
                        return "%ef";
                    case 240:
                        return "%f0";
                    case 241:
                        return "%f1";
                    case 242:
                        return "%f2";
                    case 243:
                        return "%f3";
                    case 244:
                        return "%f4";
                    case 245:
                        return "%f5";
                    case 246:
                        return "%f6";
                    case 247:
                        return "%f7";
                    case 248:
                        return "%f8";
                    case 249:
                        return "%f9";
                    case 250:
                        return "%fa";
                    case 251:
                        return "%fb";
                    case 252:
                        return "%fc";
                    case 253:
                        return "%fd";
                    case 254:
                        return "%fe";
                    case 255:
                        return "%ff";
                    default:
                        return "%3f";
                }
            }
        }

        protected abstract string urlEncode(int i);


        public static BareHttpUtility ISO_8859_1
        {
            get
            {
                return new Utility_ISO_8859_1();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <param name="into"></param>
        /// <returns>Returns the StringBuilder argument so results can be chained</returns>
        public StringBuilder UrlEncode(string str, StringBuilder into)
        {
            for (int i = 0; i < str.Length; i++)
                into.Append(urlEncode(str[i]));

            return into;
        }

        /// <summary>
        /// Encodes to a query string or form value. Doesn't include leading question mark.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public string UrlEncode(IEnumerable<KeyValuePair<string, string>> values)
        {
            return UrlEncode(values, new StringBuilder()).ToString();
        }

        public StringBuilder UrlEncode(IEnumerable<KeyValuePair<string, string>> values, StringBuilder into)
        {
            foreach (var pair in values)
            {
                UrlEncode(pair.Key, into);
                into.Append("=");
                UrlEncode(pair.Value, into);
                into.Append("&");
            }

            if (values.Any())
                into.Length--; //trim last extra & sign

            return into;
        }
    }
}