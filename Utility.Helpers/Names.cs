﻿
using System.Collections.Generic;

namespace Utility.Helpers
{
    public static class Names
    {
        static List<string> strings = new();

        public readonly static List<string> Values = new()
        {
            "Madeline Haggerty",
            "Eldon Deaver",
            "Latoria Flynn",
            "Adela Tally",
            "Rory Fuhrman",
            "Melony Lovato",
            "Nelia Maples",
            "Londa Fagin",
            "Nicki Mcleod",
            "Dante Bialaszewski",
            "Reba Vidrine",
            "Ruthie Boatman",
            "Linette Tarpey",
            "Georgianne Colangelo",
            "Jeff Roseman",
            "Halley Bormann",
            "Johnsie Vanburen",
            "Maximo Okada",
            "Marion Alverson",
            "Lottie Rhinehart",
            "Maddie Beckmann",
            "Cordell Carper",
            "Joanne Gibbon",
            "Tianna Gilfillan",
            "Hyman Guttierrez",
            "Cheri Duclos",
            "Adrienne Labrum",
            "Ciera Hurston",
            "Tamra Bohling",
            "Major Bray",
            "Debbra Leith",
            "Claudine Swensen",
            "Modesto Cebula",
            "Maryann Hosch",
            "Annemarie Brutus",
            "Lena Bowerman",
            "Parker Rentfro",
            "Nathaniel Markel",
            "Huey Mefford",
            "Justina Cushenberry",
            "Lashaun Hunsicker",
            "Raymonde Mcfate",
            "Kiesha Jenny",
            "Peggy Vroman",
            "Reinaldo Vella",
            "Margot Starck",
            "Octavia Maddem",
            "Dreama Fairfield",
            "Bobby Donnelly",
            "Pattie Schaffner",
            "Carylon Cheever",
            "Chana Stuber",
            "Bong Myatt",
            "Porfirio Garney",
            "Leann Toki",
            "Becki Feinberg",
            "Jonna Stallone",
            "Ilene Blizzard",
            "Yasuko Prigge",
            "Diego Farone",
            "Anh Gerace",
            "Isa Stukes",
            "Evelina Patz",
            "Shanell Raschke",
            "Bella Borda",
            "Laureen Jarmon",
            "Mariam Keever",
            "Olga Corle",
            "Otis Turner",
            "Katrice Hisle",
            "Corey Newnam",
            "Adan Dosch",
            "Sonny Patricio",
            "Sanda Arner",
            "Rolando Fitzgerald",
            "Annie Gatchell",
            "Suanne Bosque",
            "Anibal Terrio",
            "Alethea Guay",
            "Wilburn Takacs",
            "Forrest Aburto",
            "Zana Sikorski",
            "Wendi Gosselin",
            "Savannah Duggins",
            "Carrol Kerr",
            "Freeda Pharis",
            "Beaulah Santini",
            "Coy Dowd",
            "Kareem Arterburn",
            "Annamae Fortner",
            "Maxwell Bordelon",
            "Julietta Barajas",
            "Darcel Schur",
            "Vena Auten",
            "Audrea Roher",
            "Emory Olaughlin",
            "Deandrea Purinton",
            "Kacy Macek",
            "Lona Engler",
            "Oren Kesten",
        };


        public static string Random(this System.Random random)
        {
            string str;
            while (strings.Contains(str = Values[random.Next(0, Values.Count)]))
            {
            }
            strings.Add(str);
            return str;
        }

        //public static Names Instance { get; } = new Names();
    }
}
