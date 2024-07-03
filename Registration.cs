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
    public partial class Registration : Form
    {
        public Registration()
        {
            InitializeComponent();

        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (Логин.Text == "" || Пароль.Text == "" ||  Роль.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }
            if (!IsValidPassword(Пароль.Text))
            {
                MessageBox.Show("Пароль не соответствует требованиям:\n" +
                    "- Минимум 6 символов\n" +
                    "- Минимум 1 прописная буква\n" +
                    "- Минимум 1 цифра\n" +
                    "- Минимум один символ из набора: ! @ # $ % ^");
                return;
            }
            try
            {
                using (SqlConnection con = new SqlConnection(@"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26 "";Integrated Security=True;Connect Timeout=30;"))
                {
                    con.Open();
                    using (SqlCommand cmdCheckLogin = new SqlCommand("SELECT COUNT(*) FROM Пользователь WHERE Логин = @login", con))
                    {
                        cmdCheckLogin.Parameters.AddWithValue("@login", Логин.Text);
                        int count = (int)cmdCheckLogin.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Такой логин существует.");
                            return;
                        }
                    }                  
                    string selectedRoleName = Роль.SelectedItem.ToString();
                    int roleId = 0;
                    using (SqlCommand cmdGetRoleId = new SqlCommand("SELECT id_Роли FROM Роль WHERE НазваниеРоли = @role", con))
                    {
                        cmdGetRoleId.Parameters.AddWithValue("@role", selectedRoleName);
                        roleId = (int)cmdGetRoleId.ExecuteScalar();
                    }
                    using (SqlCommand cmdInsertUser = new SqlCommand(
                        "INSERT INTO Пользователь (Логин, Пароль) VALUES (@login, @password)", con))
                    {
                        cmdInsertUser.Parameters.AddWithValue("@login", Логин.Text);
                        cmdInsertUser.Parameters.AddWithValue("@password", Пароль.Text);
                        cmdInsertUser.ExecuteNonQuery();
                    }
                    using (SqlCommand cmdGetUserId = new SqlCommand("SELECT id_Пользователя FROM Пользователь WHERE Логин = @login", con))
                    {
                        cmdGetUserId.Parameters.AddWithValue("@login", Логин.Text);
                        int userId = (int)cmdGetUserId.ExecuteScalar();
                        using (SqlCommand cmdInsertUserRole = new SqlCommand(
                            "INSERT INTO ПользовательРоль (id_Пользователя, id_Роли) VALUES (@userId, @roleId)", con))
                        {
                            cmdInsertUserRole.Parameters.AddWithValue("@userId", userId);
                            cmdInsertUserRole.Parameters.AddWithValue("@roleId", roleId);
                            cmdInsertUserRole.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Регистрация прошла успешно.Осуществлен переход к Авторизации");
                    Avtorization form = new Avtorization(); 
                    form.Show();
                    this.Close(); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации: " + ex.Message);
            }
        }
        // Метод для проверки пароля
        private bool IsValidPassword(string password)
        {
            return password.Length >= 6 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsDigit) &&
                   password.Any(c => "!@#$%^&*".Contains(c));
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Avtorization form= new Avtorization();
            form.Show();
            this.Hide();
        }
    }
}