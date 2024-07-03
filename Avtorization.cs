using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Практика
{
    public partial class Avtorization : Form
    {
    private string connectionString = @"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26"";Integrated Security=True;Connect Timeout=30;";

        public Avtorization()
        {
            InitializeComponent();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string login = txtUsername.Text;
            string password = txtPassword.Text;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }
            if (ValidateUser(login, password))
            {
                DataTable userRoles = GetUserRoles(login);
                // Проверка разрешений 
                if (HasPermission(userRoles, "Просмотр") || HasPermission(userRoles, "Просмотр формы zadania"))
                {
                    zadania form1 = new zadania();
                    form1.Show();
                    this.Hide();
                }
                else if (HasPermission(userRoles, "Добавление") || HasPermission(userRoles, "Добавление на форме zadania"))
                {
                    zadania form1 = new zadania();
                    form1.Show();
                    this.Hide();
                }
                else if (HasPermission(userRoles, "Просмотр формы domzadania"))
                {
                    domzadania form2 = new domzadania();
                    form2.Show();
                    this.Hide();
                }
                else if (HasPermission(userRoles, "Добавление на форме domzadania"))
                {
                    domzadania form2 = new domzadania();
                    form2.Show();
                    this.Hide();
                }
                else if (HasPermission(userRoles, "Просмотр формы Prosmotr"))
                {
                    Prosmotr form3 = new Prosmotr();
                    form3.Show();
                    this.Hide();
                }
                else if (HasPermission(userRoles, "Добавление на форме Prosmotr"))
                {
                    Prosmotr form3 = new Prosmotr();
                    form3.Show();
                    this.Hide();
                }
                else if (HasPermission(userRoles, "Просмотр формы Otchet"))
                {
                    Otchet form4 = new Otchet();
                    form4.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("У вас недостаточно прав для доступа к этой форме!");
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }
        private bool ValidateUser(string login, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT 1 FROM Пользователь WHERE Логин = @login AND Пароль = @password";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);
                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
        }
        private DataTable GetUserRoles(string login)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT p.ПолноеИмя, r.НазваниеРоли
                            FROM Пользователь p
                            JOIN ПользовательРоль pr ON p.id_Пользователя = pr.id_Пользователя
                            JOIN Роль r ON pr.id_Роли = r.id_Роли
                            WHERE p.Логин = @login";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable roles = new DataTable();
                        adapter.Fill(roles);
                        return roles;
                    }
                }
            }
        }
        // Проверка наличия разрешения у роли пользователя
        private bool HasPermission(DataTable userRoles, string permissionName)
        {
            foreach (DataRow row in userRoles.Rows)
            {
                string roleName = row["НазваниеРоли"].ToString();              
                if (RoleHasPermission(roleName, permissionName))
                {
                    return true;
                }
            }
            return false;
        }
        // Проверка наличия разрешения у роли
        private bool RoleHasPermission(string roleName, string permissionName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT 1 
                            FROM Роль r 
                            JOIN РольРазрешения rr ON r.id_Роли = rr.id_Роли
                            JOIN Разрешения p ON rr.id_Разрешения = p.id_Разрешения
                            WHERE r.НазваниеРоли = @roleName AND p.Название = @permissionName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@roleName", roleName);
                    command.Parameters.AddWithValue("@permissionName", permissionName);
                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
        }
    private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Registration form=new Registration();
            form.Show();
            this.Hide();
        }
    }
}
