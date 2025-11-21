using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LLMCraftedBlazorSamples.Shared.Models;

/// <summary>
/// Represents a group from the Oracle ZOCLINIC.GROUPS_OLD table
/// </summary>
[Table("GROUPS_OLD", Schema = "ZOCLINIC")]
public class Group
{
    [Key]
    [Column("GROUP_ID")]
    [Display(Name = "Group ID")]
    public int GroupId { get; set; }

    [Column("GROUP_CODE")]
    [StringLength(4)]
    [Display(Name = "Group Code")]
    public string? GroupCode { get; set; }

    [Column("GROUP_NAME")]
    [Required]
    [StringLength(100)]
    [Display(Name = "Group Name")]
    public string GroupName { get; set; } = string.Empty;

    [Column("DESCRIPTION")]
    [StringLength(100)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Column("CREATED_DATE")]
    [Display(Name = "Created Date")]
    public DateTime? CreatedDate { get; set; }

    [Column("LAST_UPDATE")]
    [Display(Name = "Last Update")]
    public DateTime? LastUpdate { get; set; }

    [Column("COMPANY_ID")]
    [Display(Name = "Company ID")]
    public int? CompanyId { get; set; }

    [Column("ADDRESS_1")]
    [StringLength(100)]
    [Display(Name = "Address")]
    public string? Address1 { get; set; }

    [Column("ADDRESS_2")]
    [StringLength(100)]
    [Display(Name = "Address 2")]
    public string? Address2 { get; set; }

    [Column("CITY")]
    [StringLength(50)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [Column("STATE")]
    [StringLength(20)]
    [Display(Name = "State/Province")]
    public string? State { get; set; }

    [Column("ZIP")]
    [StringLength(20)]
    [Display(Name = "Zip/Post Code")]
    public string? Zip { get; set; }

    /// <summary>
    /// Gets the status display text based on data (assuming Active by default)
    /// </summary>
    [NotMapped]
    public string Status => "Active";
}
