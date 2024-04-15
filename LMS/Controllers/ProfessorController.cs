﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.VisualBasic;
using NuGet.Protocol;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            // Get all enrollments of certain class
            var enrollments =
                (from course in db.Courses
                 where course.Number == num && course.Listing == subject
                 join cls in db.Classes on course.CId equals cls.CId
                 into classes
                 from c in classes
                 where c.Year == year && c.Season == season
                 select c.Enrollments).First();

            // join that with students
            var students =
                from student in enrollments
                select new
                {
                    fname = student.StudentNavigation.FirstName,
                    lname = student.StudentNavigation.LastName,
                    uid = student.StudentNavigation.UId,
                    dob = student.StudentNavigation.Dob,
                    grade = student.Grade
                };

            return Json(students.ToArray());
        }

        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category == null)
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
                        submissions =
                            (from e in db.Submissions
                            where e.AId == a2.AId
                            select e).Count()
                    };
                return Json(ass.ToArray());
            } 
            else
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
                    where ac2.Name == category
                    join a1 in db.Assignments on ac2.AcId equals a1.AcId
                    into assignments
                    from a2 in assignments
                    select new  
                    {
                        aname = a2.Name,
                        cname = ac2.Name,
                        due = a2.Due,
                        submissions =
                            (from e in db.Submissions
                            where e.AId == a2.AId
                            select e).Count()
                    };
                return Json(ass.ToArray());
            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var cat =
                from course in db.Courses
                where course.Number == num && course.Listing == subject
                join class1 in db.Classes on course.CId equals class1.CId
                into classes
                from class2 in classes
                where class2.Year == year && class2.Season == season
                join ac1 in db.AssignmentCategories on class2.ClassId equals ac1.ClassId
                into assignmentCategories
                from ac2 in assignmentCategories
                select new
                {
                    name = ac2.Name,
                    weight = ac2.Weight,
                };
            return Json(cat.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            int classID =
                (from course in db.Courses
                where course.Number == num && course.Listing == subject
                join class1 in db.Classes on course.CId equals class1.CId
                into classes
                from class2 in classes
                where class2.Year == year && class2.Season == season
                select class2.ClassId).First();

            AssignmentCategory ac = new()
            {
                Name = category,
                Weight = Convert.ToByte(catweight),
                ClassId = classID
            };

            db.AssignmentCategories.Add(ac);
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
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var cat =
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
                select ac2).First();

            Assignment a = new()
            {
                Name = asgname,
                Points = Convert.ToUInt16(asgpoints),
                Contents = asgcontents,
                Due = asgdue,
                AcId = cat.AcId
            };

            db.Assignments.Add(a);
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
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var subs =
                from course in db.Courses
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
                join s1 in db.Submissions on a2.AId equals s1.AId
                into submissions
                from s2 in submissions
                select new
                {
                    fname = s2.StudentNavigation.FirstName,
                    lname = s2.StudentNavigation.LastName,
                    uid = s2.StudentNavigation.UId,
                    time = s2.SubmittedOn,
                    score = s2.Score // might have issues with null here
                };

            return Json(subs.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
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
                join s1 in db.Submissions on a2.AId equals s1.AId
                into submissions
                from s2 in submissions
                where s2.Student == uid
                select s2).First().Score = Convert.ToUInt16(score);

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
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var myClasses = 
                from c in db.Classes
                where c.Teacher == uid
                select new
                {
                    subject = c.CIdNavigation.Listing,
                    number = c.CIdNavigation.Number,
                    name = c.CIdNavigation.Name,
                    season = c.Season,
                    year = c.Year
                };
            return Json(myClasses.ToArray());
        }



        /*******End code to modify********/
    }
}

