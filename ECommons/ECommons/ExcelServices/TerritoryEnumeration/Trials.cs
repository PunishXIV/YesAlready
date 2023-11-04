using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ExcelServices.TerritoryEnumeration
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public static class Trials
    {
        public const ushort the_Dragons_Neck = 142;
        public const ushort the_Whorleater_Hard = 281;
        public const ushort the_Bowl_of_Embers_Hard = 292;
        public const ushort the_Navel_Hard = 293;
        public const ushort the_Howling_Eye_Hard = 294;
        public const ushort the_Bowl_of_Embers_Extreme = 295;
        public const ushort the_Navel_Extreme = 296;
        public const ushort the_Howling_Eye_Extreme = 297;
        public const ushort the_Minstrels_Ballad_Ultimas_Bane = 348;
        public const ushort Special_Event_I = 353;
        public const ushort Special_Event_II = 354;
        public const ushort the_Whorleater_Extreme = 359;
        public const ushort Thornmarch_Extreme = 364;
        public const ushort A_Relic_Reborn_the_Chimera = 368;
        public const ushort A_Relic_Reborn_the_Hydra = 369;
        public const ushort the_Striking_Tree_Hard = 374;
        public const ushort the_Striking_Tree_Extreme = 375;
        public const ushort the_Akh_Afah_Amphitheatre_Hard = 377;
        public const ushort the_Akh_Afah_Amphitheatre_Extreme = 378;
        public const ushort Urths_Fount = 394;
        public const ushort the_Chrysalis = 426;
        public const ushort Thok_ast_Thok_Hard = 432;
        public const ushort the_Limitless_Blue_Hard = 436;
        public const ushort the_Singularity_Reactor = 437;
        public const ushort Thok_ast_Thok_Extreme = 446;
        public const ushort the_Limitless_Blue_Extreme = 447;
        public const ushort the_Minstrels_Ballad_Thordans_Reign = 448;
        public const ushort Special_Event_III = 509;
        public const ushort Containment_Bay_S1T7 = 517;
        public const ushort Containment_Bay_S1T7_Extreme = 524;
        public const ushort the_Final_Steps_of_Faith = 559;
        public const ushort the_Minstrels_Ballad_Nidhoggs_Rage = 566;
        public const ushort Containment_Bay_P1T6 = 576;
        public const ushort Containment_Bay_P1T6_Extreme = 577;
        public const ushort Containment_Bay_Z1T9 = 637;
        public const ushort Containment_Bay_Z1T9_Extreme = 638;
        public const ushort the_Pool_of_Tribute = 674;
        public const ushort the_Pool_of_Tribute_Extreme = 677;
        public const ushort the_Royal_Menagerie = 679;
        public const ushort Emanation = 719;
        public const ushort Emanation_Extreme = 720;
        public const ushort the_Minstrels_Ballad_Shinryus_Domain = 730;
        public const ushort the_Jade_Stoa = 746;
        public const ushort the_Jade_Stoa_Extreme = 758;
        public const ushort the_Great_Hunt = 761;
        public const ushort the_Great_Hunt_Extreme = 762;
        public const ushort Castrum_Fluminis = 778;
        public const ushort the_Minstrels_Ballad_Tsukuyomis_Pain = 779;
        public const ushort Kugane_Ohashi = 806;
        public const ushort Hells_Kier = 810;
        public const ushort Hells_Kier_Extreme = 811;
        public const ushort the_Wreath_of_Snakes = 824;
        public const ushort the_Wreath_of_Snakes_Extreme = 825;
        public const ushort The_Dancing_Plague = 845;
        public const ushort The_Crown_of_the_Immaculate = 846;
        public const ushort The_Dying_Gasp = 847;
        public const ushort the_Crown_of_the_Immaculate_Extreme = 848;
        public const ushort the_Dancing_Plague_Extreme = 858;
        public const ushort The_Minstrels_Ballad_Hadess_Elegy = 885;
        public const ushort Cinder_Drift = 897;
        public const ushort Cinder_Drift_Extreme = 912;
        public const ushort Memoria_Misera_Extreme = 913;
        public const ushort the_Seat_of_Sacrifice = 922;
        public const ushort the_Seat_of_Sacrifice_Extreme = 923;
        public const ushort Castrum_Marinum = 934;
        public const ushort Castrum_Marinum_Extreme = 935;
        public const ushort The_Cloud_Deck = 950;
        public const ushort The_Cloud_Deck_Extreme = 951;
        public const ushort The_Dark_Inside = 992;
        public const ushort The_Minstrels_Ballad_Zodiarks_Fall = 993;
        public const ushort The_Mothercrystal = 995;
        public const ushort The_Minstrels_Ballad_Hydaelyns_Call = 996;
        public const ushort The_Final_Day = 997;
        public const ushort The_Minstrels_Ballad_Endsingers_Aria = 998;
        public const ushort the_Bowl_of_Embers = 1045;
        public const ushort the_Navel = 1046;
        public const ushort the_Howling_Eye = 1047;
        public const ushort the_Porta_Decumana = 1048;
        public const ushort Thornmarch_Hard = 1067;
        public const ushort Storms_Crown = 1071;
        public const ushort Storms_Crown_Extreme = 1072;
        public const ushort Containment_Bay_S1T7_Unreal = 1090;

        static ushort[] list = null;
        public static ushort[] List
        {
            get
            {
                if (list == null)
                {
                    var s = new List<ushort>();
                    typeof(Trials).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Each(x => s.Add((ushort)x.GetValue(null)));
                    list = s.ToArray();
                }
                return list;
            }
        }
    }
}
