using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var depos =
                from d in db.Departments
                select new
                {
                    name = d.Name,
                    subject = d.Subject 
                };
            return Json(depos.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var catalog =
                from d in db.Departments
                select new
                {
                    subject = d.Subject,
                    dname = d.Name,
                    courses = from c in d.Courses
                              select new
                              {
                                  number = c.Number,
                                  cname = c.Name
                              }
                };
            return Json(catalog.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            // get all classes from a subject and number
            var classes =
                (from course in db.Courses
                 where course.Number == number && course.Listing == subject
                 select course.Classes).First();

            // Get teachers for each of the classes in the list above
            var joined =
                from classs in classes
                join professor in db.Professors on classs.Teacher equals professor.UId
                into classWithMoreDetails
                from detailedClass in classWithMoreDetails
                select new
                {
                    season = classs.Season,
                    year = classs.Year,
                    location = classs.Location,
                    start = classs.StartTime,
                    end = classs.EndTime,
                    fname = detailedClass.FirstName,
                    lname = detailedClass.LastName
                };
            return Json(joined.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            /*// Get course offereing ID
            int cid =
            (from c in db.Courses
                 where c.Number == num && c.Listing == subject
                 select c.CId).First();

            // Get class id
            int classID =
                (from c in db.Classes
                 where c.CId == cid && c.Season == season && c.Year == year
                 select c.ClassId).First();

            // Get assignment category
            int acID =
                (from ac in db.AssignmentCategories
                 where ac.ClassId == classID && ac.Name == category
                 select ac.AcId).First();

            // Get assignment ID
            Assignment assignment = 
                (from a in db.Assignments
                 where a.AcId == acID && a.Name == asgname
                 select a).First();

            return Content(assignment.Contents);*/
            return Content((
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
                select a2.Contents).First());
        }

        // Note, if the above method works, I could simplify the lower method too

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            // Get course offereing ID
            int cid =
            (from c in db.Courses
             where c.Number == num && c.Listing == subject
             select c.CId).First();

            // Get class id
            int classID =
                (from c in db.Classes
                 where c.CId == cid && c.Season == season && c.Year == year
                 select c.ClassId).First();

            // Get assignment category
            int acID =
                (from ac in db.AssignmentCategories
                 where ac.ClassId == classID && ac.Name == category
                 select ac.AcId).First();

            // Get assignment ID
            int aID =
                (from a in db.Assignments
                 where a.AcId == acID && a.Name == asgname
                 select a.AId).First();

            // Get submission
            Submission? sub = 
                (from s in db.Submissions
                 where s.Student == uid && s.AId == aID
                 select s).FirstOrDefault();

            return Content(sub == null ? "" : sub.Contents ?? "");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var student = db.Students.Find(uid);
            if (student != null)
            {
                return Json(new{fname = student.FirstName, lname = student.LastName, uid = student.UId, department = student.Major});
            }

            var professor = db.Professors.Find(uid);
            if (professor != null)
            {
                return Json(new { fname = professor.FirstName, lname = professor.LastName, uid = professor.UId, department = professor.WorksIn });
            }

            var admin = db.Administrators.Find(uid);
            if(admin != null)
            {
                return Json(new { fname = admin.FirstName, lname = admin.LastName, uid = admin.UId });
            }

            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

