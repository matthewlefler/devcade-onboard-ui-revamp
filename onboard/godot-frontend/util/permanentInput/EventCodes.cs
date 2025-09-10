namespace onboard.util.permenentInput;
public static class EventTypes
{
    /*
    * Event types
    */
    public static readonly int EV_SYN = 0x00;
    public static readonly int EV_KEY = 0x01;
    public static readonly int EV_REL = 0x02;
    public static readonly int EV_ABS = 0x03;
    public static readonly int EV_MSC = 0x04;
    public static readonly int EV_SW = 0x05;
    public static readonly int EV_LED = 0x11;
    public static readonly int EV_SND = 0x12;
    public static readonly int EV_REP = 0x14;
    public static readonly int EV_FF = 0x15;
    public static readonly int EV_PWR = 0x16;
    public static readonly int EV_FF_STATUS = 0x17;
    public static readonly int EV_MAX = 0x1f;
    public static readonly int EV_CNT = (EV_MAX + 1);
}

public class EventCodes
{
    /*
     * Device properties and quirks
    */
    public static readonly int INPUT_PROP_POINTER = 0x00;	/* needs a pointer */
    public static readonly int INPUT_PROP_DIRECT = 0x01;	/* direct input devices */
    public static readonly int INPUT_PROP_BUTTONPAD = 0x02;	/* has button(s) under pad */
    public static readonly int INPUT_PROP_SEMI_MT = 0x03;	/* touch rectangle only */
    public static readonly int INPUT_PROP_TOPBUTTONPAD = 0x04;	/* softbuttons at top of pad */
    public static readonly int INPUT_PROP_POINTING_STICK = 0x05;	/* is a pointing stick */
    public static readonly int INPUT_PROP_ACCELEROMETER = 0x06;	/* has accelerometer */
    public static readonly int INPUT_PROP_MAX = 0x1f;
    public static readonly int INPUT_PROP_CNT = (INPUT_PROP_MAX + 1);

    /*
    * Synchronization events.
    */
    public static readonly int SYN_REPORT = 0;
    public static readonly int SYN_CONFIG = 1;
    public static readonly int SYN_MT_REPORT = 2;
    public static readonly int SYN_DROPPED = 3;
    public static readonly int SYN_MAX = 0xf;
    public static readonly int SYN_CNT = (SYN_MAX + 1);
    /*
    * Keys and buttons
    *
    * Most of the keys/buttons are modeled after USB HUT 1.12
    * (see http://www.usb.org/developers/hidpage).
    * Abbreviations in the comments:
    * AC - Application Control
    * AL - Application Launch Button
    * SC - System Control
    */
    public static readonly int KEY_RESERVED = 0;
    public static readonly int KEY_ESC = 1;
    public static readonly int KEY_1 = 2;
    public static readonly int KEY_2 = 3;
    public static readonly int KEY_3 = 4;
    public static readonly int KEY_4 = 5;
    public static readonly int KEY_5 = 6;
    public static readonly int KEY_6 = 7;
    public static readonly int KEY_7 = 8;
    public static readonly int KEY_8 = 9;
    public static readonly int KEY_9 = 10;
    public static readonly int KEY_0 = 11;
    public static readonly int KEY_MINUS = 12;
    public static readonly int KEY_EQUAL = 13;
    public static readonly int KEY_BACKSPACE = 14;
    public static readonly int KEY_TAB = 15;
    public static readonly int KEY_Q = 16;
    public static readonly int KEY_W = 17;
    public static readonly int KEY_E = 18;
    public static readonly int KEY_R = 19;
    public static readonly int KEY_T = 20;
    public static readonly int KEY_Y = 21;
    public static readonly int KEY_U = 22;
    public static readonly int KEY_I = 23;
    public static readonly int KEY_O = 24;
    public static readonly int KEY_P = 25;
    public static readonly int KEY_LEFTBRACE = 26;
    public static readonly int KEY_RIGHTBRACE = 27;
    public static readonly int KEY_ENTER = 28;
    public static readonly int KEY_LEFTCTRL = 29;
    public static readonly int KEY_A = 30;
    public static readonly int KEY_S = 31;
    public static readonly int KEY_D = 32;
    public static readonly int KEY_F = 33;
    public static readonly int KEY_G = 34;
    public static readonly int KEY_H = 35;
    public static readonly int KEY_J = 36;
    public static readonly int KEY_K = 37;
    public static readonly int KEY_L = 38;
    public static readonly int KEY_SEMICOLON = 39;
    public static readonly int KEY_APOSTROPHE = 40;
    public static readonly int KEY_GRAVE = 41;
    public static readonly int KEY_LEFTSHIFT = 42;
    public static readonly int KEY_BACKSLASH = 43;
    public static readonly int KEY_Z = 44;
    public static readonly int KEY_X = 45;
    public static readonly int KEY_C = 46;
    public static readonly int KEY_V = 47;
    public static readonly int KEY_B = 48;
    public static readonly int KEY_N = 49;
    public static readonly int KEY_M = 50;
    public static readonly int KEY_COMMA = 51;
    public static readonly int KEY_DOT = 52;
    public static readonly int KEY_SLASH = 53;
    public static readonly int KEY_RIGHTSHIFT = 54;
    public static readonly int KEY_KPASTERISK = 55;
    public static readonly int KEY_LEFTALT = 56;
    public static readonly int KEY_SPACE = 57;
    public static readonly int KEY_CAPSLOCK = 58;
    public static readonly int KEY_F1 = 59;
    public static readonly int KEY_F2 = 60;
    public static readonly int KEY_F3 = 61;
    public static readonly int KEY_F4 = 62;
    public static readonly int KEY_F5 = 63;
    public static readonly int KEY_F6 = 64;
    public static readonly int KEY_F7 = 65;
    public static readonly int KEY_F8 = 66;
    public static readonly int KEY_F9 = 67;
    public static readonly int KEY_F10 = 68;
    public static readonly int KEY_NUMLOCK = 69;
    public static readonly int KEY_SCROLLLOCK = 70;
    public static readonly int KEY_KP7 = 71;
    public static readonly int KEY_KP8 = 72;
    public static readonly int KEY_KP9 = 73;
    public static readonly int KEY_KPMINUS = 74;
    public static readonly int KEY_KP4 = 75;
    public static readonly int KEY_KP5 = 76;
    public static readonly int KEY_KP6 = 77;
    public static readonly int KEY_KPPLUS = 78;
    public static readonly int KEY_KP1 = 79;
    public static readonly int KEY_KP2 = 80;
    public static readonly int KEY_KP3 = 81;
    public static readonly int KEY_KP0 = 82;
    public static readonly int KEY_KPDOT = 83;
    public static readonly int KEY_ZENKAKUHANKAKU = 85;
    public static readonly int KEY_102ND = 86;
    public static readonly int KEY_F11 = 87;
    public static readonly int KEY_F12 = 88;
    public static readonly int KEY_RO = 89;
    public static readonly int KEY_KATAKANA = 90;
    public static readonly int KEY_HIRAGANA = 91;
    public static readonly int KEY_HENKAN = 92;
    public static readonly int KEY_KATAKANAHIRAGANA = 93;
    public static readonly int KEY_MUHENKAN = 94;
    public static readonly int KEY_KPJPCOMMA = 95;
    public static readonly int KEY_KPENTER = 96;
    public static readonly int KEY_RIGHTCTRL = 97;
    public static readonly int KEY_KPSLASH = 98;
    public static readonly int KEY_SYSRQ = 99;
    public static readonly int KEY_RIGHTALT = 100;
    public static readonly int KEY_LINEFEED = 101;
    public static readonly int KEY_HOME = 102;
    public static readonly int KEY_UP = 103;
    public static readonly int KEY_PAGEUP = 104;
    public static readonly int KEY_LEFT = 105;
    public static readonly int KEY_RIGHT = 106;
    public static readonly int KEY_END = 107;
    public static readonly int KEY_DOWN = 108;
    public static readonly int KEY_PAGEDOWN = 109;
    public static readonly int KEY_INSERT = 110;
    public static readonly int KEY_DELETE = 111;
    public static readonly int KEY_MACRO = 112;
    public static readonly int KEY_MUTE = 113;
    public static readonly int KEY_VOLUMEDOWN = 114;
    public static readonly int KEY_VOLUMEUP = 115;
    public static readonly int KEY_POWER = 116;	/* SC System Power Down */
    public static readonly int KEY_KPEQUAL = 117;
    public static readonly int KEY_KPPLUSMINUS = 118;
    public static readonly int KEY_PAUSE = 119;
    public static readonly int KEY_SCALE = 120;	/* AL Compiz Scale (Expose) */
    public static readonly int KEY_KPCOMMA = 121;
    public static readonly int KEY_HANGEUL = 122;
    public static readonly int KEY_HANGUEL = KEY_HANGEUL;
    public static readonly int KEY_HANJA = 123;
    public static readonly int KEY_YEN = 124;
    public static readonly int KEY_LEFTMETA = 125;
    public static readonly int KEY_RIGHTMETA = 126;
    public static readonly int KEY_COMPOSE = 127;
    public static readonly int KEY_STOP = 128; /* AC Stop */
    public static readonly int KEY_AGAIN = 129;
    public static readonly int KEY_PROPS = 130;	/* AC Properties */
    public static readonly int KEY_UNDO = 131;	/* AC Undo */
    public static readonly int KEY_FRONT = 132;
    public static readonly int KEY_COPY = 133;	/* AC Copy */
    public static readonly int KEY_OPEN = 134;	/* AC Open */
    public static readonly int KEY_PASTE = 135;	/* AC Paste */
    public static readonly int KEY_FIND = 136;	/* AC Search */
    public static readonly int KEY_CUT = 137;	/* AC Cut */
    public static readonly int KEY_HELP = 138;	/* AL public static readonly integrated Help Center */
    public static readonly int KEY_MENU = 139;	/* Menu (show menu) */
    public static readonly int KEY_CALC = 140;	/* AL Calculator */
    public static readonly int KEY_SETUP = 141;
    public static readonly int KEY_SLEEP = 142;	/* SC System Sleep */
    public static readonly int KEY_WAKEUP = 143;	/* System Wake Up */
    public static readonly int KEY_FILE = 144;	/* AL Local Machine Browser */
    public static readonly int KEY_SENDFILE = 145;
    public static readonly int KEY_DELETEFILE = 146;
    public static readonly int KEY_XFER = 147;
    public static readonly int KEY_PROG1 = 148;
    public static readonly int KEY_PROG2 = 149;
    public static readonly int KEY_WWW = 150;	/* AL public static readonly internet Browser */
    public static readonly int KEY_MSDOS = 151;
    public static readonly int KEY_COFFEE = 152;	/* AL Terminal Lock/Screensaver */
    public static readonly int KEY_SCREENLOCK = KEY_COFFEE;
    public static readonly int KEY_ROTATE_DISPLAY = 153;	/* Display orientation for e.g. tablets */
    public static readonly int KEY_DIRECTION = KEY_ROTATE_DISPLAY;
    public static readonly int KEY_CYCLEWINDOWS = 154;
    public static readonly int KEY_MAIL = 155;
    public static readonly int KEY_BOOKMARKS = 156;	/* AC Bookmarks */
    public static readonly int KEY_COMPUTER = 157;
    public static readonly int KEY_BACK = 158;	/* AC Back */
    public static readonly int KEY_FORWARD = 159;	/* AC Forward */
    public static readonly int KEY_CLOSECD = 160;
    public static readonly int KEY_EJECTCD = 161;
    public static readonly int KEY_EJECTCLOSECD = 162;
    public static readonly int KEY_NEXTSONG = 163;
    public static readonly int KEY_PLAYPAUSE = 164;
    public static readonly int KEY_PREVIOUSSONG = 165;
    public static readonly int KEY_STOPCD = 166;
    public static readonly int KEY_RECORD = 167;
    public static readonly int KEY_REWIND = 168;
    public static readonly int KEY_PHONE = 169;	/* Media Select Telephone */
    public static readonly int KEY_ISO = 170;
    public static readonly int KEY_CONFIG = 171;	/* AL Consumer Control Configuration */
    public static readonly int KEY_HOMEPAGE = 172;	/* AC Home */
    public static readonly int KEY_REFRESH = 173;	/* AC Refresh */
    public static readonly int KEY_EXIT = 174;	/* AC Exit */
    public static readonly int KEY_MOVE = 175;
    public static readonly int KEY_EDIT = 176;
    public static readonly int KEY_SCROLLUP = 177;
    public static readonly int KEY_SCROLLDOWN = 178;
    public static readonly int KEY_KPLEFTPAREN = 179;
    public static readonly int KEY_KPRIGHTPAREN = 180;
    public static readonly int KEY_NEW = 181;	/* AC New */
    public static readonly int KEY_REDO = 182;	/* AC Redo/Repeat */
    public static readonly int KEY_F13 = 183;
    public static readonly int KEY_F14 = 184;
    public static readonly int KEY_F15 = 185;
    public static readonly int KEY_F16 = 186;
    public static readonly int KEY_F17 = 187;
    public static readonly int KEY_F18 = 188;
    public static readonly int KEY_F19 = 189;
    public static readonly int KEY_F20 = 190;
    public static readonly int KEY_F21 = 191;
    public static readonly int KEY_F22 = 192;
    public static readonly int KEY_F23 = 193;
    public static readonly int KEY_F24 = 194;
    public static readonly int KEY_PLAYCD = 200;
    public static readonly int KEY_PAUSECD = 201;
    public static readonly int KEY_PROG3 = 202;
    public static readonly int KEY_PROG4 = 203;
    public static readonly int KEY_DASHBOARD = 204;	/* AL Dashboard */
    public static readonly int KEY_SUSPEND = 205;
    public static readonly int KEY_CLOSE = 206;	/* AC Close */
    public static readonly int KEY_PLAY = 207;
    public static readonly int KEY_FASTFORWARD = 208;
    public static readonly int KEY_BASSBOOST = 209;
    public static readonly int KEY_PRINT = 210;	/* AC Print */
    public static readonly int KEY_HP = 211;
    public static readonly int KEY_CAMERA = 212;
    public static readonly int KEY_SOUND = 213;
    public static readonly int KEY_QUESTION = 214;
    public static readonly int KEY_EMAIL = 215;
    public static readonly int KEY_CHAT = 216;
    public static readonly int KEY_SEARCH = 217;
    public static readonly int KEY_CONNECT = 218;
    public static readonly int KEY_FINANCE = 219;	/* AL Checkbook/Finance */
    public static readonly int KEY_SPORT = 220;
    public static readonly int KEY_SHOP = 221;
    public static readonly int KEY_ALTERASE = 222;
    public static readonly int KEY_CANCEL = 223;	/* AC Cancel */
    public static readonly int KEY_BRIGHTNESSDOWN = 224;
    public static readonly int KEY_BRIGHTNESSUP = 225;
    public static readonly int KEY_MEDIA = 226;
    public static readonly int KEY_SWITCHVIDEOMODE = 227;	/* Cycle between available video outputs (Monitor/LCD/TV-out/etc) */
    public static readonly int KEY_KBDILLUMTOGGLE = 228;
    public static readonly int KEY_KBDILLUMDOWN = 229;
    public static readonly int KEY_KBDILLUMUP = 230;
    public static readonly int KEY_SEND = 231;	/* AC Send */
    public static readonly int KEY_REPLY = 232;	/* AC Reply */
    public static readonly int KEY_FORWARDMAIL = 233;	/* AC Forward Msg */
    public static readonly int KEY_SAVE = 234;	/* AC Save */
    public static readonly int KEY_DOCUMENTS = 235;
    public static readonly int KEY_BATTERY = 236;
    public static readonly int KEY_BLUETOOTH = 237;
    public static readonly int KEY_WLAN = 238;
    public static readonly int KEY_UWB = 239;
    public static readonly int KEY_UNKNOWN = 240;
    public static readonly int KEY_VIDEO_NEXT = 241;	/* drive next video source */
    public static readonly int KEY_VIDEO_PREV = 242;	/* drive previous video source */
    public static readonly int KEY_BRIGHTNESS_CYCLE = 243;	/* brightness up, after max is min */
    public static readonly int KEY_BRIGHTNESS_AUTO = 244;	/* Set Auto Brightness: manual brightness control is off, rely on ambient */
    public static readonly int KEY_BRIGHTNESS_ZERO = KEY_BRIGHTNESS_AUTO;
    public static readonly int KEY_DISPLAY_OFF = 245;	/* display device to off state */
    public static readonly int KEY_WWAN = 246;	/* Wireless WAN (LTE, UMTS, GSM, etc.) */
    public static readonly int KEY_WIMAX = KEY_WWAN;
    public static readonly int KEY_RFKILL = 247;	/* Key that controls all radios */
    public static readonly int KEY_MICMUTE = 248;	/* Mute / unmute the microphone */

    /* Code 255 is reserved for special needs of AT keyboard driver */

    public static readonly int BTN_MISC = 0x100;
    public static readonly int BTN_0 = 0x100;
    public static readonly int BTN_1 = 0x101;
    public static readonly int BTN_2 = 0x102;
    public static readonly int BTN_3 = 0x103;
    public static readonly int BTN_4 = 0x104;
    public static readonly int BTN_5 = 0x105;
    public static readonly int BTN_6 = 0x106;
    public static readonly int BTN_7 = 0x107;
    public static readonly int BTN_8 = 0x108;
    public static readonly int BTN_9 = 0x109;
    public static readonly int BTN_MOUSE = 0x110;
    public static readonly int BTN_LEFT = 0x110;
    public static readonly int BTN_RIGHT = 0x111;
    public static readonly int BTN_MIDDLE = 0x112;
    public static readonly int BTN_SIDE = 0x113;
    public static readonly int BTN_EXTRA = 0x114;
    public static readonly int BTN_FORWARD = 0x115;
    public static readonly int BTN_BACK = 0x116;
    public static readonly int BTN_TASK = 0x117;
    public static readonly int BTN_JOYSTICK = 0x120;
    public static readonly int BTN_TRIGGER = 0x120;
    public static readonly int BTN_THUMB = 0x121;
    public static readonly int BTN_THUMB2 = 0x122;
    public static readonly int BTN_TOP = 0x123;
    public static readonly int BTN_TOP2 = 0x124;
    public static readonly int BTN_PINKIE = 0x125;
    public static readonly int BTN_BASE = 0x126;
    public static readonly int BTN_BASE2 = 0x127;
    public static readonly int BTN_BASE3 = 0x128;
    public static readonly int BTN_BASE4 = 0x129;
    public static readonly int BTN_BASE5 = 0x12a;
    public static readonly int BTN_BASE6 = 0x12b;
    public static readonly int BTN_DEAD = 0x12f;
    public static readonly int BTN_GAMEPAD = 0x130;
    public static readonly int BTN_SOUTH = 0x130;
    public static readonly int BTN_A = BTN_SOUTH;
    public static readonly int BTN_EAST = 0x131;
    public static readonly int BTN_B = BTN_EAST;
    public static readonly int BTN_C = 0x132;
    public static readonly int BTN_NORTH = 0x133;
    public static readonly int BTN_X = BTN_NORTH;
    public static readonly int BTN_WEST = 0x134;
    public static readonly int BTN_Y = BTN_WEST;
    public static readonly int BTN_Z = 0x135;
    public static readonly int BTN_TL = 0x136;
    public static readonly int BTN_TR = 0x137;
    public static readonly int BTN_TL2 = 0x138;
    public static readonly int BTN_TR2 = 0x139;
    public static readonly int BTN_SELECT = 0x13a;
    public static readonly int BTN_START = 0x13b;
    public static readonly int BTN_MODE = 0x13c;
    public static readonly int BTN_THUMBL = 0x13d;
    public static readonly int BTN_THUMBR = 0x13e;
    public static readonly int BTN_DIGI = 0x140;
    public static readonly int BTN_TOOL_PEN = 0x140;
    public static readonly int BTN_TOOL_RUBBER = 0x141;
    public static readonly int BTN_TOOL_BRUSH = 0x142;
    public static readonly int BTN_TOOL_PENCIL = 0x143;
    public static readonly int BTN_TOOL_AIRBRUSH = 0x144;
    public static readonly int BTN_TOOL_FINGER = 0x145;
    public static readonly int BTN_TOOL_MOUSE = 0x146;
    public static readonly int BTN_TOOL_LENS = 0x147;
    public static readonly int BTN_TOOL_QUINTTAP = 0x148;	/* Five fingers on trackpad */
    public static readonly int BTN_TOUCH = 0x14a;
    public static readonly int BTN_STYLUS = 0x14b;
    public static readonly int BTN_STYLUS2 = 0x14c;
    public static readonly int BTN_TOOL_DOUBLETAP = 0x14d;
    public static readonly int BTN_TOOL_TRIPLETAP = 0x14e;
    public static readonly int BTN_TOOL_QUADTAP = 0x14f;	/* Four fingers on trackpad */
    public static readonly int BTN_WHEEL = 0x150;
    public static readonly int BTN_GEAR_DOWN = 0x150;
    public static readonly int BTN_GEAR_UP = 0x151;
    public static readonly int KEY_OK = 0x160;
    public static readonly int KEY_SELECT = 0x161;
    public static readonly int KEY_GOTO = 0x162;
    public static readonly int KEY_CLEAR = 0x163;
    public static readonly int KEY_POWER2 = 0x164;
    public static readonly int KEY_OPTION = 0x165;
    public static readonly int KEY_INFO = 0x166;   /* AL OEM Features/Tips/Tutorial */
    public static readonly int KEY_TIME = 0x167;
    public static readonly int KEY_VENDOR = 0x168;
    public static readonly int KEY_ARCHIVE = 0x169;
    public static readonly int KEY_PROGRAM = 0x16a;	/* Media Select Program Guide */
    public static readonly int KEY_CHANNEL = 0x16b;
    public static readonly int KEY_FAVORITES = 0x16c;
    public static readonly int KEY_EPG = 0x16d;
    public static readonly int KEY_PVR = 0x16e;	/* Media Select Home */
    public static readonly int KEY_MHP = 0x16f;
    public static readonly int KEY_LANGUAGE = 0x170;
    public static readonly int KEY_TITLE = 0x171;
    public static readonly int KEY_SUBTITLE = 0x172;
    public static readonly int KEY_ANGLE = 0x173;
    public static readonly int KEY_ZOOM = 0x174;
    public static readonly int KEY_MODE = 0x175;
    public static readonly int KEY_KEYBOARD = 0x176;
    public static readonly int KEY_SCREEN = 0x177;
    public static readonly int KEY_PC = 0x178; /* Media Select Computer */
    public static readonly int KEY_TV = 0x179;	/* Media Select TV */
    public static readonly int KEY_TV2 = 0x17a;	/* Media Select Cable */
    public static readonly int KEY_VCR = 0x17b;	/* Media Select VCR */
    public static readonly int KEY_VCR2 = 0x17c;	/* VCR Plus */
    public static readonly int KEY_SAT = 0x17d;	/* Media Select Satellite */
    public static readonly int KEY_SAT2 = 0x17e;
    public static readonly int KEY_CD = 0x17f;	/* Media Select CD */
    public static readonly int KEY_TAPE = 0x180;	/* Media Select Tape */
    public static readonly int KEY_RADIO = 0x181;
    public static readonly int KEY_TUNER = 0x182;	/* Media Select Tuner */
    public static readonly int KEY_PLAYER = 0x183;
    public static readonly int KEY_TEXT = 0x184;
    public static readonly int KEY_DVD = 0x185;	/* Media Select DVD */
    public static readonly int KEY_AUX = 0x186;
    public static readonly int KEY_MP3 = 0x187;
    public static readonly int KEY_AUDIO = 0x188;	/* AL Audio Browser */
    public static readonly int KEY_VIDEO = 0x189;	/* AL Movie Browser */
    public static readonly int KEY_DIRECTORY = 0x18a;
    public static readonly int KEY_LIST = 0x18b;
    public static readonly int KEY_MEMO = 0x18c;	/* Media Select Messages */
    public static readonly int KEY_CALENDAR = 0x18d;
    public static readonly int KEY_RED = 0x18e;
    public static readonly int KEY_GREEN = 0x18f;
    public static readonly int KEY_YELLOW = 0x190;
    public static readonly int KEY_BLUE = 0x191;
    public static readonly int KEY_CHANNELUP = 0x192;	/* Channel Increment */
    public static readonly int KEY_CHANNELDOWN = 0x193;	/* Channel Decrement */
    public static readonly int KEY_FIRST = 0x194;
    public static readonly int KEY_LAST = 0x195;	/* Recall Last */
    public static readonly int KEY_AB = 0x196;
    public static readonly int KEY_NEXT = 0x197;
    public static readonly int KEY_RESTART = 0x198;
    public static readonly int KEY_SLOW = 0x199;
    public static readonly int KEY_SHUFFLE = 0x19a;
    public static readonly int KEY_BREAK = 0x19b;
    public static readonly int KEY_PREVIOUS = 0x19c;
    public static readonly int KEY_DIGITS = 0x19d;
    public static readonly int KEY_TEEN = 0x19e;
    public static readonly int KEY_TWEN = 0x19f;
    public static readonly int KEY_VIDEOPHONE = 0x1a0;	/* Media Select Video Phone */
    public static readonly int KEY_GAMES = 0x1a1;	/* Media Select Games */
    public static readonly int KEY_ZOOMIN = 0x1a2;	/* AC Zoom In */
    public static readonly int KEY_ZOOMOUT = 0x1a3;	/* AC Zoom Out */
    public static readonly int KEY_ZOOMRESET = 0x1a4;	/* AC Zoom */
    public static readonly int KEY_WORDPROCESSOR = 0x1a5;	/* AL Word Processor */
    public static readonly int KEY_EDITOR = 0x1a6;	/* AL Text Editor */
    public static readonly int KEY_SPREADSHEET = 0x1a7;	/* AL Spreadsheet */
    public static readonly int KEY_GRAPHICSEDITOR = 0x1a8;	/* AL Graphics Editor */
    public static readonly int KEY_PRESENTATION = 0x1a9;	/* AL Presentation App */
    public static readonly int KEY_DATABASE = 0x1aa;	/* AL Database App */
    public static readonly int KEY_NEWS = 0x1ab;	/* AL Newsreader */
    public static readonly int KEY_VOICEMAIL = 0x1ac;	/* AL Voicemail */
    public static readonly int KEY_ADDRESSBOOK = 0x1ad;	/* AL Contacts/Address Book */
    public static readonly int KEY_MESSENGER = 0x1ae;	/* AL Instant Messaging */
    public static readonly int KEY_DISPLAYTOGGLE = 0x1af;	/* Turn display (LCD) on and off */
    public static readonly int KEY_BRIGHTNESS_TOGGLE = KEY_DISPLAYTOGGLE;
    public static readonly int KEY_SPELLCHECK = 0x1b0;   /* AL Spell Check */
    public static readonly int KEY_LOGOFF = 0x1b1;   /* AL Logoff */
    public static readonly int KEY_DOLLAR = 0x1b2;
    public static readonly int KEY_EURO = 0x1b3;
    public static readonly int KEY_FRAMEBACK = 0x1b4;	/* Consumer - transport controls */
    public static readonly int KEY_FRAMEFORWARD = 0x1b5;
    public static readonly int KEY_CONTEXT_MENU = 0x1b6;	/* GenDesc - system context menu */
    public static readonly int KEY_MEDIA_REPEAT = 0x1b7;	/* Consumer - transport control */
    public static readonly int KEY_10CHANNELSUP = 0x1b8;	/* 10 channels up (10+) */
    public static readonly int KEY_10CHANNELSDOWN = 0x1b9;	/* 10 channels down (10-) */
    public static readonly int KEY_IMAGES = 0x1ba;	/* AL Image Browser */
    public static readonly int KEY_DEL_EOL = 0x1c0;
    public static readonly int KEY_DEL_EOS = 0x1c1;
    public static readonly int KEY_INS_LINE = 0x1c2;
    public static readonly int KEY_DEL_LINE = 0x1c3;
    public static readonly int KEY_FN = 0x1d0;
    public static readonly int KEY_FN_ESC = 0x1d1;
    public static readonly int KEY_FN_F1 = 0x1d2;
    public static readonly int KEY_FN_F2 = 0x1d3;
    public static readonly int KEY_FN_F3 = 0x1d4;
    public static readonly int KEY_FN_F4 = 0x1d5;
    public static readonly int KEY_FN_F5 = 0x1d6;
    public static readonly int KEY_FN_F6 = 0x1d7;
    public static readonly int KEY_FN_F7 = 0x1d8;
    public static readonly int KEY_FN_F8 = 0x1d9;
    public static readonly int KEY_FN_F9 = 0x1da;
    public static readonly int KEY_FN_F10 = 0x1db;
    public static readonly int KEY_FN_F11 = 0x1dc;
    public static readonly int KEY_FN_F12 = 0x1dd;
    public static readonly int KEY_FN_1 = 0x1de;
    public static readonly int KEY_FN_2 = 0x1df;
    public static readonly int KEY_FN_D = 0x1e0;
    public static readonly int KEY_FN_E = 0x1e1;
    public static readonly int KEY_FN_F = 0x1e2;
    public static readonly int KEY_FN_S = 0x1e3;
    public static readonly int KEY_FN_B = 0x1e4;
    public static readonly int KEY_BRL_DOT1 = 0x1f1;
    public static readonly int KEY_BRL_DOT2 = 0x1f2;
    public static readonly int KEY_BRL_DOT3 = 0x1f3;
    public static readonly int KEY_BRL_DOT4 = 0x1f4;
    public static readonly int KEY_BRL_DOT5 = 0x1f5;
    public static readonly int KEY_BRL_DOT6 = 0x1f6;
    public static readonly int KEY_BRL_DOT7 = 0x1f7;
    public static readonly int KEY_BRL_DOT8 = 0x1f8;
    public static readonly int KEY_BRL_DOT9 = 0x1f9;
    public static readonly int KEY_BRL_DOT10 = 0x1fa;
    public static readonly int KEY_NUMERIC_0 = 0x200;	/* used by phones, remote controls, */
    public static readonly int KEY_NUMERIC_1 = 0x201;	/* and other keypads */
    public static readonly int KEY_NUMERIC_2 = 0x202;
    public static readonly int KEY_NUMERIC_3 = 0x203;
    public static readonly int KEY_NUMERIC_4 = 0x204;
    public static readonly int KEY_NUMERIC_5 = 0x205;
    public static readonly int KEY_NUMERIC_6 = 0x206;
    public static readonly int KEY_NUMERIC_7 = 0x207;
    public static readonly int KEY_NUMERIC_8 = 0x208;
    public static readonly int KEY_NUMERIC_9 = 0x209;
    public static readonly int KEY_NUMERIC_STAR = 0x20a;
    public static readonly int KEY_NUMERIC_POUND = 0x20b;
    public static readonly int KEY_NUMERIC_A = 0x20c;	/* Phone key A - HUT Telephony = 0xb9 */
    public static readonly int KEY_NUMERIC_B = 0x20d;
    public static readonly int KEY_NUMERIC_C = 0x20e;
    public static readonly int KEY_NUMERIC_D = 0x20f;
    public static readonly int KEY_CAMERA_FOCUS = 0x210;
    public static readonly int KEY_WPS_BUTTON = 0x211;	/* WiFi Protected Setup key */
    public static readonly int KEY_TOUCHPAD_TOGGLE = 0x212;	/* Request switch touchpad on or off */
    public static readonly int KEY_TOUCHPAD_ON = 0x213;
    public static readonly int KEY_TOUCHPAD_OFF = 0x214;
    public static readonly int KEY_CAMERA_ZOOMIN = 0x215;
    public static readonly int KEY_CAMERA_ZOOMOUT = 0x216;
    public static readonly int KEY_CAMERA_UP = 0x217;
    public static readonly int KEY_CAMERA_DOWN = 0x218;
    public static readonly int KEY_CAMERA_LEFT = 0x219;
    public static readonly int KEY_CAMERA_RIGHT = 0x21a;
    public static readonly int KEY_ATTENDANT_ON = 0x21b;
    public static readonly int KEY_ATTENDANT_OFF = 0x21c;
    public static readonly int KEY_ATTENDANT_TOGGLE = 0x21d;	/* Attendant call on or off */
    public static readonly int KEY_LIGHTS_TOGGLE = 0x21e;	/* Reading light on or off */
    public static readonly int BTN_DPAD_UP = 0x220;
    public static readonly int BTN_DPAD_DOWN = 0x221;
    public static readonly int BTN_DPAD_LEFT = 0x222;
    public static readonly int BTN_DPAD_RIGHT = 0x223;
    public static readonly int KEY_ALS_TOGGLE = 0x230;	/* Ambient light sensor */
    public static readonly int KEY_BUTTONCONFIG = 0x240;	/* AL Button Configuration */
    public static readonly int KEY_TASKMANAGER = 0x241;	/* AL Task/Project Manager */
    public static readonly int KEY_JOURNAL = 0x242;	/* AL Log/Journal/Timecard */
    public static readonly int KEY_CONTROLPANEL = 0x243;	/* AL Control Panel */
    public static readonly int KEY_APPSELECT = 0x244;	/* AL Select Task/Application */
    public static readonly int KEY_SCREENSAVER = 0x245;	/* AL Screen Saver */
    public static readonly int KEY_VOICECOMMAND = 0x246;	/* Listening Voice Command */
    public static readonly int KEY_BRIGHTNESS_MIN = 0x250;	/* Set Brightness to Minimum */
    public static readonly int KEY_BRIGHTNESS_MAX = 0x251;	/* Set Brightness to Maximum */
    public static readonly int KEY_KBDINPUTASSIST_PREV = 0x260;
    public static readonly int KEY_KBDINPUTASSIST_NEXT = 0x261;
    public static readonly int KEY_KBDINPUTASSIST_PREVGROUP = 0x262;
    public static readonly int KEY_KBDINPUTASSIST_NEXTGROUP = 0x263;
    public static readonly int KEY_KBDINPUTASSIST_ACCEPT = 0x264;
    public static readonly int KEY_KBDINPUTASSIST_CANCEL = 0x265;
    /* Diagonal movement keys */
    public static readonly int KEY_RIGHT_UP = 0x266;
    public static readonly int KEY_RIGHT_DOWN = 0x267;
    public static readonly int KEY_LEFT_UP = 0x268;
    public static readonly int KEY_LEFT_DOWN = 0x269;
    public static readonly int KEY_ROOT_MENU = 0x26a; /* Show Device's Root Menu */
    /* Show Top Menu of the Media (e.g. DVD) */
    public static readonly int KEY_MEDIA_TOP_MENU = 0x26b;
    public static readonly int KEY_NUMERIC_11 = 0x26c;
    public static readonly int KEY_NUMERIC_12 = 0x26d;
    /*
    * Toggle Audio Description: refers to an audio service that helps blind and
    * visually impaired consumers understand the action in a program. Note: in
    * some countries this is referred to as "Video Description".
    */
    public static readonly int KEY_AUDIO_DESC = 0x26e;
    public static readonly int KEY_3D_MODE = 0x26f;
    public static readonly int KEY_NEXT_FAVORITE = 0x270;
    public static readonly int KEY_STOP_RECORD = 0x271;
    public static readonly int KEY_PAUSE_RECORD = 0x272;
    public static readonly int KEY_VOD = 0x273; /* Video on Demand */
    public static readonly int KEY_UNMUTE = 0x274;
    public static readonly int KEY_FASTREVERSE = 0x275;
    public static readonly int KEY_SLOWREVERSE = 0x276;
    /*
    * Control a data application associated with the currently viewed channel,
    * e.g. teletext or data broadcast application (MHEG, MHP, HbbTV, etc.)
    */
    public static readonly int KEY_DATA = 0x277;
    public static readonly int BTN_TRIGGER_HAPPY = 0x2c0;
    public static readonly int BTN_TRIGGER_HAPPY1 = 0x2c0;
    public static readonly int BTN_TRIGGER_HAPPY2 = 0x2c1;
    public static readonly int BTN_TRIGGER_HAPPY3 = 0x2c2;
    public static readonly int BTN_TRIGGER_HAPPY4 = 0x2c3;
    public static readonly int BTN_TRIGGER_HAPPY5 = 0x2c4;
    public static readonly int BTN_TRIGGER_HAPPY6 = 0x2c5;
    public static readonly int BTN_TRIGGER_HAPPY7 = 0x2c6;
    public static readonly int BTN_TRIGGER_HAPPY8 = 0x2c7;
    public static readonly int BTN_TRIGGER_HAPPY9 = 0x2c8;
    public static readonly int BTN_TRIGGER_HAPPY10 = 0x2c9;
    public static readonly int BTN_TRIGGER_HAPPY11 = 0x2ca;
    public static readonly int BTN_TRIGGER_HAPPY12 = 0x2cb;
    public static readonly int BTN_TRIGGER_HAPPY13 = 0x2cc;
    public static readonly int BTN_TRIGGER_HAPPY14 = 0x2cd;
    public static readonly int BTN_TRIGGER_HAPPY15 = 0x2ce;
    public static readonly int BTN_TRIGGER_HAPPY16 = 0x2cf;
    public static readonly int BTN_TRIGGER_HAPPY17 = 0x2d0;
    public static readonly int BTN_TRIGGER_HAPPY18 = 0x2d1;
    public static readonly int BTN_TRIGGER_HAPPY19 = 0x2d2;
    public static readonly int BTN_TRIGGER_HAPPY20 = 0x2d3;
    public static readonly int BTN_TRIGGER_HAPPY21 = 0x2d4;
    public static readonly int BTN_TRIGGER_HAPPY22 = 0x2d5;
    public static readonly int BTN_TRIGGER_HAPPY23 = 0x2d6;
    public static readonly int BTN_TRIGGER_HAPPY24 = 0x2d7;
    public static readonly int BTN_TRIGGER_HAPPY25 = 0x2d8;
    public static readonly int BTN_TRIGGER_HAPPY26 = 0x2d9;
    public static readonly int BTN_TRIGGER_HAPPY27 = 0x2da;
    public static readonly int BTN_TRIGGER_HAPPY28 = 0x2db;
    public static readonly int BTN_TRIGGER_HAPPY29 = 0x2dc;
    public static readonly int BTN_TRIGGER_HAPPY30 = 0x2dd;
    public static readonly int BTN_TRIGGER_HAPPY31 = 0x2de;
    public static readonly int BTN_TRIGGER_HAPPY32 = 0x2df;
    public static readonly int BTN_TRIGGER_HAPPY33 = 0x2e0;
    public static readonly int BTN_TRIGGER_HAPPY34 = 0x2e1;
    public static readonly int BTN_TRIGGER_HAPPY35 = 0x2e2;
    public static readonly int BTN_TRIGGER_HAPPY36 = 0x2e3;
    public static readonly int BTN_TRIGGER_HAPPY37 = 0x2e4;
    public static readonly int BTN_TRIGGER_HAPPY38 = 0x2e5;
    public static readonly int BTN_TRIGGER_HAPPY39 = 0x2e6;
    public static readonly int BTN_TRIGGER_HAPPY40 = 0x2e7;
    /* We avoid low common keys in module aliases so they don't get huge. */
    public static readonly int KEY_MIN_INTERESTING = KEY_MUTE;
    public static readonly int KEY_MAX = 0x2ff;
    public static readonly int KEY_CNT = (KEY_MAX + 1);
    /*
    * Relative axes
    */
    public static readonly int REL_X = 0x00;
    public static readonly int REL_Y = 0x01;
    public static readonly int REL_Z = 0x02;
    public static readonly int REL_RX = 0x03;
    public static readonly int REL_RY = 0x04;
    public static readonly int REL_RZ = 0x05;
    public static readonly int REL_HWHEEL = 0x06;
    public static readonly int REL_DIAL = 0x07;
    public static readonly int REL_WHEEL = 0x08;
    public static readonly int REL_MISC = 0x09;
    public static readonly int REL_MAX = 0x0f;
    public static readonly int REL_CNT = (REL_MAX + 1);
    /*
    * Absolute axes
    */
    public static readonly int ABS_X = 0x00;
    public static readonly int ABS_Y = 0x01;
    public static readonly int ABS_Z = 0x02;
    public static readonly int ABS_RX = 0x03;
    public static readonly int ABS_RY = 0x04;
    public static readonly int ABS_RZ = 0x05;
    public static readonly int ABS_THROTTLE = 0x06;
    public static readonly int ABS_RUDDER = 0x07;
    public static readonly int ABS_WHEEL = 0x08;
    public static readonly int ABS_GAS = 0x09;
    public static readonly int ABS_BRAKE = 0x0a;
    public static readonly int ABS_HAT = 0x10;
    public static readonly int ABS_HAT0Y = 0x11;
    public static readonly int ABS_HAT1X = 0x12;
    public static readonly int ABS_HAT1Y = 0x13;
    public static readonly int ABS_HAT2X = 0x14;
    public static readonly int ABS_HAT2Y = 0x15;
    public static readonly int ABS_HAT3X = 0x16;
    public static readonly int ABS_HAT3Y = 0x17;
    public static readonly int ABS_PRESSURE = 0x18;
    public static readonly int ABS_DISTANCE = 0x19;
    public static readonly int ABS_TILT_X = 0x1a;
    public static readonly int ABS_TILT_Y = 0x1b;
    public static readonly int ABS_TOOL_WIDTH = 0x1c;
    public static readonly int ABS_VOLUME = 0x20;
    public static readonly int ABS_MISC = 0x28;
    /*
    * = 0x2e is reserved and should not be used in input drivers.
    * It was used by HID as ABS_MISC+6 and userspace needs to detect if
    * the next ABS_* event is correct or is just ABS_MISC + n.
    * We define here ABS_RESERVED so userspace can rely on it and detect
    * the situation described above.
    */
    public static readonly int ABS_RESERVED = 0x2e;
    public static readonly int ABS_MT_SLOT = 0x2f;	/* MT slot being modified */
    public static readonly int ABS_MT_TOUCH_MAJOR = 0x30;	/* Major axis of touching ellipse */
    public static readonly int ABS_MT_TOUCH_MINOR = 0x31;	/* Minor axis (omit if circular) */
    public static readonly int ABS_MT_WIDTH_MAJOR = 0x32;	/* Major axis of approaching ellipse */
    public static readonly int ABS_MT_WIDTH_MINOR = 0x33;	/* Minor axis (omit if circular) */
    public static readonly int ABS_MT_ORIENTATION = 0x34;	/* Ellipse orientation */
    public static readonly int ABS_MT_POSITION_X = 0x35;	/* Center X touch position */
    public static readonly int ABS_MT_POSITION_Y = 0x36;	/* Center Y touch position */
    public static readonly int ABS_MT_TOOL_TYPE = 0x37;	/* Type of touching device */
    public static readonly int ABS_MT_BLOB_ID = 0x38;	/* Group a set of packets as a blob */
    public static readonly int ABS_MT_TRACKING_ID = 0x39;	/* Unique ID of initiated contact */
    public static readonly int ABS_MT_PRESSURE = 0x3a;	/* Pressure on contact area */
    public static readonly int ABS_MT_DISTANCE = 0x3b;	/* Contact hover distance */
    public static readonly int ABS_MT_TOOL_X = 0x3c;	/* Center X tool position */
    public static readonly int ABS_MT_TOOL_Y = 0x3d;	/* Center Y tool position */
    public static readonly int ABS_MAX = 0x3f;
    public static readonly int ABS_CNT = (ABS_MAX + 1);
    /*
    * Switch events
    */
    public static readonly int SW_LID = 0x00;  /* set = lid shut */
    public static readonly int SW_TABLET_MODE = 0x01;  /* set = tablet mode */
    public static readonly int SW_HEADPHONE_INSERT = 0x02;  /* set = inserted */
    public static readonly int SW_RFKILL_ALL = 0x03;  /* rfkill master switch, type "any"
                        set = radio enabled */
    public static readonly int SW_RADIO = SW_RFKILL_ALL; /* deprecated */
    public static readonly int SW_MICROPHONE_INSERT = 0x04;  /* set = inserted */
    public static readonly int SW_DOCK = 0x05;  /* set = plugged public static readonly into dock */
    public static readonly int SW_LINEOUT_INSERT = 0x06;  /* set = inserted */
    public static readonly int SW_JACK_PHYSICAL_INSERT = 0x07;  /* set = mechanical switch set */
    public static readonly int SW_VIDEOOUT_INSERT = 0x08;  /* set = inserted */
    public static readonly int SW_CAMERA_LENS_COVER = 0x09;  /* set = lens covered */
    public static readonly int SW_KEYPAD_SLIDE = 0x0a;  /* set = keypad slide out */
    public static readonly int SW_FRONT_PROXIMITY = 0x0b;  /* set = front proximity sensor active */
    public static readonly int SW_ROTATE_LOCK = 0x0c;  /* set = rotate locked/disabled */
    public static readonly int SW_LINEIN_INSERT = 0x0d;  /* set = inserted */
    public static readonly int SW_MUTE_DEVICE = 0x0e;  /* set = device disabled */
    public static readonly int SW_PEN_INSERTED = 0x0f;  /* set = pen inserted */
    public static readonly int SW_MAX = 0x0f;
    public static readonly int SW_CNT = (SW_MAX + 1);
    /*
    * Misc events
    */
    public static readonly int MSC_SERIAL = 0x00;
    public static readonly int MSC_PULSELED = 0x01;
    public static readonly int MSC_GESTURE = 0x02;
    public static readonly int MSC_RAW = 0x03;
    public static readonly int MSC_SCAN = 0x04;
    public static readonly int MSC_TIMESTAMP = 0x05;
    public static readonly int MSC_MAX = 0x07;
    public static readonly int MSC_CNT = (MSC_MAX + 1);
    /*
    * LEDs
    */
    public static readonly int LED_NUML = 0x00;
    public static readonly int LED_CAPSL = 0x01;
    public static readonly int LED_SCROLLL = 0x02;
    public static readonly int LED_COMPOSE = 0x03;
    public static readonly int LED_KANA = 0x04;
    public static readonly int LED_SLEEP = 0x05;
    public static readonly int LED_SUSPEND = 0x06;
    public static readonly int LED_MUTE = 0x07;
    public static readonly int LED_MISC = 0x08;
    public static readonly int LED_MAIL = 0x09;
    public static readonly int LED_CHARGING = 0x0a;
    public static readonly int LED_MAX = 0x0f;
    public static readonly int LED_CNT = (LED_MAX + 1);
    /*
    * Autorepeat values
    */
    public static readonly int REP_DELAY = 0x00;
    public static readonly int REP_PERIOD = 0x01;
    public static readonly int REP_MAX = 0x01;
    public static readonly int REP_CNT = (REP_MAX + 1);
    /*
    * Sounds
    */
    public static readonly int SND_CLICK = 0x00;
    public static readonly int SND_BELL = 0x01;
    public static readonly int SND_TONE = 0x02;
    public static readonly int SND_MAX = 0x07;
    public static readonly int SND_CNT = (SND_MAX + 1);
}