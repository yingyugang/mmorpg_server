using System.Collections;
using System.Collections.Generic;
using System;

public static class StringColor
{
    private static string SystemColor(this string self, string _color) { return string.Format("<color={0}>{1}</color>", _color, self); }
    public static string HEXColor(this string self, string _hex) { return SystemColor(self, "#"+_hex); }
    public static string RedColor(this string self) { return self.HEXColor("ff0000"); }
    public static string AliceblueColor(this string self) { return self.HEXColor("f0f8ff"); }
    public static string AquaColor(this string self) { return self.HEXColor("00ffff"); }
    public static string AquaMarineColor(this string self) { return self.HEXColor("7fffd4"); }
    public static string BlueColor(this string self) { return self.HEXColor("#0000FF"); }
    public static string YellowColor(this string self) { return SystemColor(self, "yellow"); }
    // public static string  (this string self) { return self.HEXColor("#"); }
}
