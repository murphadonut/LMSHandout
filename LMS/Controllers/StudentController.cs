﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classes = 
                from e in db.Enrollments
                where e.Student == uid
                select new
                {
                    subject = e.ClassNavigation.CIdNavigation.Listing,
                    number = e.ClassNavigation.CIdNavigation.Number,
                    name = e.ClassNavigation.CIdNavigation.Name,
                    season = e.ClassNavigation.Season,
                    year = e.ClassNavigation.Year,
                    grade = e.Grade
                };
            return Json(classes.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var ass =
                from course in db.Courses
                where course.Number == num && course.Listing == subject
                join class1 in db.Classes on course.CId equals class1.CId
                into classes
                from class2 in classes
                where class2.Year == year && class2.Season == season
                join ac1 in db.AssignmentCategories on class2.ClassId equals ac1.ClassId
                into assignmentCategories
                from ac2 in assignmentCategories
                join a1 in db.Assignments on ac2.AcId equals a1.AcId
                into assignments
                from a2 in assignments
                select new
                {
                    aname = a2.Name,
                    cname = ac2.Name,
                    due = a2.Due,
                    score = 
                        (from s in db.Submissions
                        where s.AId == a2.AId && s.Student == uid
                        select s.Score).First() ?? 0
                };
            return Json(ass.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            int assID =
                (from course in db.Courses
                where course.Number == num && course.Listing == subject
                join class1 in db.Classes on course.CId equals class1.CId
                into classes
                from class2 in classes
                where class2.Year == year && class2.Season == season
                join ac1 in db.AssignmentCategories on class2.ClassId equals ac1.ClassId
                into assignmentCategories
                from ac2 in assignmentCategories
                where ac2.Name == category
                join a1 in db.Assignments on ac2.AcId equals a1.AcId
                into assignments
                from a2 in assignments
                where a2.Name == asgname
                select a2.AId).First();

            Submission? sub =
                (from s in db.Submissions
                where s.AId == assID && s.Student == uid
                select s).FirstOrDefault();

            if(sub == null)
            {
                sub = new()
                {
                    Contents = contents,
                    AId = assID,
                    Student = uid,
                    Score = 0
                };

                db.Submissions.Add(sub);
            }
            else
            {
                sub.Contents = contents;
                sub.SubmittedOn = DateTime.Now;
            }

            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            int classID =
                (from course in db.Courses
                where course.Number == num && course.Listing == subject
                join class1 in db.Classes on course.CId equals class1.CId
                into classes
                from class2 in classes
                where class2.Year == year && class2.Season == season
                select class2.ClassId).First();

            Enrollment e = new()
            {
                Student = uid,
                Grade = "--",
                Class = classID,
            };

            db.Enrollments.Add(e);
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var enrollments =
                from e in db.Enrollments
                where e.Student == uid
                select e;

            double gpa = 0;
            int classCount = 0;
            if (enrollments != null)
            
                foreach (var enrollment in enrollments)
                {
                    if(!enrollment.Grade.Equals("--"))
                    {
                        classCount++;
                        switch (enrollment.Grade)
                        {
                            case "A":
                                gpa += 16;
                                break;
                            case "A-":
                                gpa += 14.8;
                                break;
                            case "B+":
                                gpa += 13.2;
                                break;
                            case "B":
                                gpa += 12;
                                break;
                            case "B-":
                                gpa += 10.8;
                                break;
                            case "C+":
                                gpa += 9.2;
                                break;
                            case "C":
                                gpa += 8;
                                break;
                            case "C-":
                                gpa += 6.8;
                                break;
                            case "D+":
                                gpa += 5.2;
                                break;
                            case "D":
                                gpa += 4.0;
                                break;
                            case "D-":
                                gpa += 2.8;
                                break;
                        }
                    }                
                gpa = gpa / (4 * classCount);
            }

            System.Diagnostics.Debug.WriteLine("GPA is sdfsdfsdf: " + gpa);
            return Json(new { gpa = gpa });
        }
                
        /*******End code to modify********/

    }
}

