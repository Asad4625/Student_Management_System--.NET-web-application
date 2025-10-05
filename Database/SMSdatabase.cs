using SMSProject.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;

namespace SMSProject.Database
{
    public class SMSdatabase
    {
        private readonly string connectionString = "Data Source=Administrator\\MSSQLSERVERASAD1;Initial Catalog = BSSE6BWEProject; Persist Security Info=True;User ID = sa; Password=1234;Encrypt=false";

        public int InsertAdmin(AdminRegistration admin)
        {
            int affectedRows = 0;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();

                    string query = @"INSERT INTO Admin (Name, CNIC, Phone, City, Email, Password, Photo) 
                                   VALUES (@Name, @CNIC, @Phone, @City, @Email, @Password, @Photo)";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@Name", admin.Name);
                        cmd.Parameters.AddWithValue("@CNIC", admin.CNIC);
                        cmd.Parameters.AddWithValue("@Phone", admin.Phone);
                        cmd.Parameters.AddWithValue("@City", admin.City);
                        cmd.Parameters.AddWithValue("@Email", admin.Email);
                        cmd.Parameters.AddWithValue("@Password", admin.Password);
                        cmd.Parameters.AddWithValue("@Photo", admin.Photo);

                        affectedRows = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Insert Error : " + ex.Message);
            }

            return affectedRows;
        }

        public bool CheckEmailExists(string email)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    string query = "SELECT COUNT(*) FROM Admin WHERE Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("CheckEmail Error: " + ex.Message);
                return false;
            }
        }

        //admin login function
        public bool ValidateAdminLogin(string cnic, string password)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    string query = "SELECT COUNT(*) FROM Admin WHERE CNIC = @CNIC AND Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@CNIC", cnic);
                        cmd.Parameters.AddWithValue("@Password", password);

                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Login Error: " + ex.Message);
                return false;
            }
        }


        public AdminRegistration GetAdminByCNIC(string cnic)
        {
            AdminRegistration admin = null;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Admin WHERE CNIC = @CNIC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CNIC", cnic);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    admin = new AdminRegistration
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        CNIC = reader["CNIC"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        City = reader["City"].ToString(),
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(),
                        Photo = reader["Photo"] != DBNull.Value ? (byte[])reader["Photo"] : null
                    };
                }

                reader.Close();
            }

            return admin;
        }



        //StudentSignup
        public int InsertStudentSignup(StudentSignup studentSignup)
        {
            int affectedRows = 0;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();

                    string query = "INSERT INTO StudentSignup (StudentID, Password, Status) VALUES (@StudentID, @Password, 'Pending')";

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@StudentId", studentSignup.StudentID);
                        cmd.Parameters.AddWithValue("@Password", studentSignup.Password); // For security, hash it!

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Insert Error : " + ex.Message);
            }

            return affectedRows;
        }

        public bool CheckStudentidExists(string StudentID)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    string query = "SELECT COUNT(*) FROM StudentSignup WHERE StudentID = @StudentID";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@StudentID", StudentID);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("CheckStudentID Error: " + ex.Message);
                return false;
            }
        }

        //Student login function
        public bool ValidateStudentLogin(string StudentID, string Password)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    string query = "SELECT COUNT(*) FROM StudentSignup WHERE StudentID = @StudentID AND Password = @Password AND Status = 'Approved'";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@StudentID", StudentID);
                        cmd.Parameters.AddWithValue("@Password", Password);

                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Login Error: " + ex.Message);
                return false;
            }
        }


        // Insert course function
        public bool InsertCourse(Course course)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Courses (CourseCode, CourseName, Department, Credits, Instructor, Semester, Capacity,  Description)
                                 VALUES (@CourseCode, @CourseName, @Department, @Credits, @Instructor, @Semester, @Capacity,  @Description)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CourseCode", course.CourseCode);
                cmd.Parameters.AddWithValue("@CourseName", course.CourseName);
                cmd.Parameters.AddWithValue("@Department", course.Department);
                cmd.Parameters.AddWithValue("@Credits", course.Credits);
                cmd.Parameters.AddWithValue("@Instructor", course.Instructor);
                cmd.Parameters.AddWithValue("@Semester", course.Semester);
                cmd.Parameters.AddWithValue("@Capacity", course.Capacity);
                cmd.Parameters.AddWithValue("@Description", course.Description);

                con.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }
        

        public  List<Course> GetAllCourses()
        {
            List<Course> courses = new List<Course>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Courses"; // Table name should match your DB

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Course course = new Course
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CourseCode = reader["CourseCode"].ToString(),
                        CourseName = reader["CourseName"].ToString(),
                        Department = reader["Department"].ToString(),
                        Credits = Convert.ToInt32(reader["Credits"]),
                        Instructor = reader["Instructor"].ToString(),
                        Semester = Convert.ToInt32(reader["Semester"]),
                        Capacity = Convert.ToInt32(reader["Capacity"]),
                        Description = reader["Description"].ToString()
                    };

                    courses.Add(course);
                }
            }

            return courses;
        }
        public bool UpdateCourse(Course updatedCourse)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"UPDATE Courses SET 
                            CourseCode = @CourseCode,
                            CourseName = @CourseName,
                            Department = @Department,
                            Credits = @Credits,
                            Instructor = @Instructor,
                            Semester = @Semester,
                            Capacity = @Capacity,
                            Description = @Description
                         WHERE Id = @CourseId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CourseId", updatedCourse.Id);
                cmd.Parameters.AddWithValue("@CourseCode", updatedCourse.CourseCode);
                cmd.Parameters.AddWithValue("@CourseName", updatedCourse.CourseName);
                cmd.Parameters.AddWithValue("@Department", updatedCourse.Department);
                cmd.Parameters.AddWithValue("@Credits", updatedCourse.Credits);
                cmd.Parameters.AddWithValue("@Instructor", updatedCourse.Instructor);
                cmd.Parameters.AddWithValue("@Semester", updatedCourse.Semester);
                cmd.Parameters.AddWithValue("@Capacity", updatedCourse.Capacity);
                cmd.Parameters.AddWithValue("@Description", updatedCourse.Description);
               

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public bool DeleteCourse(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Courses WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }



        // Insert student function
        public bool InsertStudent(Student student)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Students 
            (StudentID, Name, CNIC, Address, City, Gender, Email, Phone, Photo)
            VALUES 
            (@StudentID, @Name, @CNIC, @Address, @City, @Gender, @Email, @Phone, @Photo)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@StudentID", student.StudentID);
                cmd.Parameters.AddWithValue("@Name", student.Name);
                cmd.Parameters.AddWithValue("@CNIC", student.CNIC);
                cmd.Parameters.AddWithValue("@Address", student.Address);
                cmd.Parameters.AddWithValue("@City", student.City);
                cmd.Parameters.AddWithValue("@Gender", student.Gender);
                cmd.Parameters.AddWithValue("@Email", student.Email);
                cmd.Parameters.AddWithValue("@Phone", student.Phone);
                cmd.Parameters.AddWithValue("@Photo", (object)student.Photo ?? DBNull.Value);

                con.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public List<Student> GetAllStudents()
        {
            List<Student> students = new List<Student>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Students";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Student student = new Student
                    {
                        StudentID = reader["StudentID"].ToString(),
                        Name = reader["Name"].ToString(),
                        CNIC = reader["CNIC"].ToString(),
                        Address = reader["Address"].ToString(),
                        City = reader["City"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Photo = reader["Photo"] == DBNull.Value ? null : (byte[])reader["Photo"]
                    };

                    students.Add(student);
                }
            }

            return students;
        }

        public bool UpdateStudent(Student updatedStudent)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"UPDATE Students SET 
                Name = @Name,
                CNIC = @CNIC,
                Address = @Address,
                City = @City,
                Gender = @Gender,
                Email = @Email,
                Phone = @Phone,
                Photo = @Photo
            WHERE StudentID = @StudentID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@StudentID", updatedStudent.StudentID);
                cmd.Parameters.AddWithValue("@Name", updatedStudent.Name);
                cmd.Parameters.AddWithValue("@CNIC", updatedStudent.CNIC);
                cmd.Parameters.AddWithValue("@Address", updatedStudent.Address);
                cmd.Parameters.AddWithValue("@City", updatedStudent.City);
                cmd.Parameters.AddWithValue("@Gender", updatedStudent.Gender);
                cmd.Parameters.AddWithValue("@Email", updatedStudent.Email);
                cmd.Parameters.AddWithValue("@Phone", updatedStudent.Phone);
                cmd.Parameters.AddWithValue("@Photo", (object)updatedStudent.Photo ?? DBNull.Value);

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteStudent(string studentId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Students WHERE StudentID = @StudentID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@StudentID", studentId);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }




        //here started enrollment 
        public bool InsertEnrollment(Enrollment enrollment)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Enrollment 
                        (EnrollmentID, StudentID, CourseID, Department, EnrollDate, Semester, CoursesName, Status)
                         VALUES 
                        (@EnrollmentID, @StudentID, @CourseID, @Department, @EnrollDate, @Semester, @CoursesName, @Status)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@EnrollmentID", enrollment.EnrollmentID);
                cmd.Parameters.AddWithValue("@StudentID", enrollment.StudentID);
                cmd.Parameters.AddWithValue("@CourseID", enrollment.CourseID);
                cmd.Parameters.AddWithValue("@Department", enrollment.Department);
                cmd.Parameters.AddWithValue("@EnrollDate", enrollment.EnrollDate);
                cmd.Parameters.AddWithValue("@Semester", enrollment.Semester);
                cmd.Parameters.AddWithValue("@CoursesName", enrollment.CoursesName);
                cmd.Parameters.AddWithValue("@Status", enrollment.Status);

                con.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public List<Enrollment> GetAllEnrollments()
        {
            List<Enrollment> enrollments = new List<Enrollment>();
           

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Enrollment"; // Table name should match your DB
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Enrollment enrollment = new Enrollment
                    {
                        EnrollmentID = reader["EnrollmentID"].ToString(),
                        StudentID = reader["StudentID"].ToString(),
                        CourseID = Convert.ToInt32(reader["CourseID"]),
                        Department = reader["Department"].ToString(),
                        EnrollDate = Convert.ToDateTime(reader["EnrollDate"]),
                        Semester = Convert.ToInt32(reader["Semester"]),
                        CoursesName = reader["CoursesName"].ToString(),
                        Status = reader["Status"].ToString()
                    };

                    enrollments.Add(enrollment);
                }
            }

            return enrollments;
        }

        public bool DeleteEnrollment(string studentId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Enrollment WHERE EnrollmentID = @EnrollmentID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@EnrollmentID", studentId);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public Enrollment GetEnrollmentById(string id)
        {
            Enrollment enrollment = null;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT * FROM Enrollment WHERE EnrollmentID = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        enrollment = new Enrollment
                        {
                            EnrollmentID = reader["EnrollmentID"].ToString(),
                            StudentID = reader["StudentID"].ToString(),
                            CourseID = Convert.ToInt32(reader["CourseID"]),
                            Department = reader["Department"].ToString(),
                            EnrollDate = (DateTime)reader["EnrollDate"],
                            Semester = Convert.ToInt32(reader["Semester"]),
                            CoursesName = reader["CoursesName"].ToString(),
                            Status = reader["Status"].ToString()
                        };
                    }
                }
            }

            return enrollment;
        }

        public  bool UpdateStatus(string enrollmentId, string courseName, string status)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Enrollment SET Status = @Status WHERE EnrollmentID = @EnrollmentID AND CoursesName = @CoursesName";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@EnrollmentID", enrollmentId); // ✅ MATCH THIS NAME EXACTLY
                cmd.Parameters.AddWithValue("@CoursesName", courseName);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public Student GetStudentById(string StudentId)
        {
            Student student = null;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Students WHERE StudentID = @StudentID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@StudentID", StudentId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            student = new Student
                            {
                                StudentID = reader["StudentID"].ToString(),
                                Name = reader["Name"].ToString(),
                                CNIC = reader["CNIC"].ToString(),
                                Address = reader["Address"].ToString(),
                                City = reader["City"].ToString(),
                                Gender = reader["Gender"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                Photo = reader["Photo"] != DBNull.Value ? (byte[])reader["Photo"] : null
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("GetStudentById Error: " + ex.Message);
            }
            return student;
        }

















        public List<StudentSignup> GetPendingSignups()
        {
            List<StudentSignup> pendingSignups = new List<StudentSignup>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM StudentSignup";
                    SqlCommand cmd = new SqlCommand(query, con);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pendingSignups.Add(new StudentSignup
                            {
                                StudentID = reader["StudentID"].ToString(),
                                Password = reader["Password"].ToString(),
                                Status = reader["Status"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("GetPendingSignups Error: " + ex.Message);
            }
            return pendingSignups;
        }

        public bool UpdateSignupStatus(string studentId, string status)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE StudentSignup SET Status = @Status WHERE StudentID = @StudentID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@StudentID", studentId);

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("UpdateSignupStatus Error: " + ex.Message);
                return false;
            }
        }
    }
}

