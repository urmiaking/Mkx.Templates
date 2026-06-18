using System;
using System.Globalization;

namespace Mkx.Templates.Sdk.Shared.Utilities;

public static class CultureHelper
{
    public static CultureInfo GetPersianCulture()
    {
        return new CultureInfo("fa-IR")
        {
            NumberFormat =
            {
                NegativeSign = "-",
                NumberDecimalSeparator = ".",
                CurrencySymbol = "ریال",
                CurrencyPositivePattern = 3, // Symbol on the left with a space  
                CurrencyNegativePattern = 8, // Symbol on the left with a space for negative values  
                NumberGroupSeparator = ",",     // Explicitly set this
                CurrencyDecimalSeparator = ".", // Explicitly set this (was defaulting to /)
                CurrencyGroupSeparator = ",",   // Explicitly set this
            },
            DateTimeFormat =
            {
                ShortDatePattern = "yyyy/MM/dd",
                LongDatePattern = "dddd, dd MMMM yyyy",
                FirstDayOfWeek = DayOfWeek.Saturday,
                ShortestDayNames = ["ی", "د", "س", "چ", "پ", "ج", "ش"],
                DayNames = ["یکشنبه", "دوشنبه", "سه شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه"],
                MonthNames =
                [
                    "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن",
                    "اسفند", string.Empty
                ],
                MonthGenitiveNames =
                [
                    "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن",
                    "اسفند", string.Empty
                ],
                AbbreviatedMonthGenitiveNames =
                [
                    "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن",
                    "اسفند", string.Empty
                ],
                AbbreviatedMonthNames =
                [
                    "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن",
                    "اسفند", string.Empty
                ]
            }
        };
    }
}
