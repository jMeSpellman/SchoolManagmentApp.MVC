using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SchoolManagmentApp.MVC.Data;

public class StudentMetadata
{
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Data Of Birth")]
    public DateOnly? DataOfBirth { get; set; }
}

[ModelMetadataType(typeof(StudentMetadata))]
public partial class Student { }
