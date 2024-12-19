using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SchoolManagmentApp.MVC.Data;

public class ClassMetadata
{
    [Display(Name = "Lecturer")]
    public int LecturerId { get; set; }

    [Display(Name = "Course")]
    public int CorseId { get; set; }
}

[ModelMetadataType(typeof(ClassMetadata))]
public partial class Class { }
