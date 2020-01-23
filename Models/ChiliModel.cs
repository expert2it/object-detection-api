using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CiliAPI.NetCore.Models
{
    public class ChiliModel
    {
        public int Id { get; set; }
        public string CheckedBy { get; set; }
        public string Datetime { get; set; }
        public string PlantId { get; set; }
        public string TreeHeight { get; set; }
        public double? NumberOfFruits { get; set; }
        public double? NumberOfLeaves { get; set; }
        public string LeafLength { get; set; }
        public string LeafWidth { get; set; }
        public string LeafColor { get; set; }
        public string FruitLength { get; set; }
        public string FruitWidth { get; set; }
        public string FruitWeight { get; set; }
        public string FruitColor { get; set; }
        public string FruitMaturityIndex { get; set; }
        public string ChiliGrading { get; set; }
        public string NutrientIron { get; set; }
        public string NutrientIronScale { get; set; }
        public string NutrientNitrogen { get; set; }
        public string NutrientNitrogenScale { get; set; }
        public string NutrientPotassium { get; set; }
        public string NutrientPotassiumScale { get; set; }
        public string NutrientPhosporus { get; set; }
        public string NutrientPhosporusScale { get; set; }
        public string NutrientSulphur { get; set; }
        public string NutrientSulphurScale { get; set; }
        public string BacterialSpot { get; set; }
        public string BacterialSpotScale { get; set; }
        public string FungalFusariumWilt { get; set; }
        public string FungalFusariumWiltScale { get; set; }
        public string FungalPowderyMildew { get; set; }
        public string FungalPowderyMildewScale { get; set; }
        public string FungalSouthernBlight { get; set; }
        public string FungalSouthernBlightScale { get; set; }
        public string VirusMosaic { get; set; }
        public string VirusMosaicScale { get; set; }
        public string PestAphids { get; set; }
        public string PestAphidsScale { get; set; }
        public string PestBeetArmyworm { get; set; }
        public string PestBeetArmywormScale { get; set; }
        public string PestFleaBeetles { get; set; }
        public string PestFleaBeetlesScale { get; set; }
        public string PestLeafminers { get; set; }
        public string PestLeafminersScale { get; set; }
        public string PestFruitWorm { get; set; }
        public string PestFruitWormScale { get; set; }
        public string PestSpiderMites { get; set; }
        public string PestSpiderMitesScale { get; set; }
        public string PestWhitefly { get; set; }
        public string PestWhiteflyScale { get; set; }
        public string FruitGradeMaturity { get; set; }
        public string FruitGradeInfection { get; set; }
        public string FruitGradeWeightSize { get; set; }
        public string PlantAge { get; set; }
        public string Image { get; set; }
        public DateTimeOffset? LastUpdate { get; set; }
    }
}
