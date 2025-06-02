using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventManagerWorker.Utility.Enum
{
    public enum CampaignEnum
    {
        /// <summary>
        /// No
        /// </summary>
        [Display(Name = "No")]
        [Description("No")]
        No,

        /// <summary>
        /// Inclusive
        /// </summary>
        [Display(Name = "Inclusive")]
        [Description("Inclusive")]
        Inclusive,

        /// <summary>
        /// Exclusive
        /// </summary>
        [Display(Name = "Exclusive")]
        [Description("Exclusive")]
        Exclusive,

        /// <summary>
        /// Inclusion
        /// </summary>
        [Display(Name = "Inclusion")]
        [Description("Inclusion")]
        Inclusion,

        /// <summary>
        /// Exclusion
        /// </summary>
        [Display(Name = "Exclusion")]
        [Description("Exclusion")]
        Exclusion,

        /// <summary>
        /// Exclusion
        /// </summary>
        [Display(Name = "Internal")]
        [Description("Internal")]
        Internal,

        /// <summary>
        /// Exclusion
        /// </summary>
        [Display(Name = "External")]
        [Description("External")]
        External

    }
}
