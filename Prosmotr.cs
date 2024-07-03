using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Практика
{
    public partial class Prosmotr : Form
    {
        SqlConnection con; 
        SqlCommand cmd;
        SqlDataReader dr;
        public Prosmotr()
        {
            InitializeComponent();
            FillComboBoxes();
            DisplayZadania();
            Статус.Items.Add("Одобрен");
            Статус.Items.Add("Отклонен");
            Статус.Items.Add("Обработка");
            Статус.Items.Add("Готов");
        }  
        string connectionString = @"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26"";Integrated Security=True;Connect Timeout=30;";
        int currentUserId;
        private void DisplayZadania()
        {
            try
            {
                con = new SqlConnection(connectionString);
                con.Open();
                string Query = @"
            SELECT 
                z.id_задания, 
                z.тема_урока, 
                z.описание_задания, 
                z.срок_сдачи, 
                z.статус,
                o.ответ AS Ответы,
                u.ПолноеИмя AS Имя_ученика,
                c.название_класса AS Класс_ученика,
                o.файл_ответа AS ФайлОтвета 
            FROM 
                Задания z
            LEFT JOIN 
                Ответы o ON z.id_задания = o.id_задания
            LEFT JOIN 
                Ученики uc ON z.класс_id = uc.класс_id
            LEFT JOIN 
                Пользователь u ON uc.id_Пользователя = u.id_Пользователя
            LEFT JOIN 
                Классы c ON uc.класс_id = c.id";

                cmd = new SqlCommand(Query, con);
                dr = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dataGridView1.DataSource = dt;
                if (dataGridView1.Columns.Contains("файл_задания"))
                {
                    dataGridView1.Columns["файл_задания"].Visible = false;
                }
                dt.Columns.Add("Имя файла", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    if (row["ФайлОтвета"] != DBNull.Value)
                    {
                        string fileName = Path.GetFileName(row["ФайлОтвета"].ToString());
                        row["Имя файла"] = fileName;
                    }
                }
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void FillComboBoxes()
        {
            try
            {
                con = new SqlConnection(connectionString);
                con.Open();   
                cmd = new SqlCommand("SELECT DISTINCT название_класса FROM Классы", con);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Класс.Items.Add(dr["название_класса"]); 
                }
                dr.Close();
                cmd = new SqlCommand("SELECT DISTINCT ПолноеИмя FROM Пользователь", con); 
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Ученик.Items.Add(dr["ПолноеИмя"]); 
                }
                dr.Close(); 
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }
    private void button6_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите задание для изменения статуса!");
                return;
            }
            try
            {
                int taskId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
                string newStatus = Статус.SelectedItem.ToString();
                UpdateTaskStatus(taskId, newStatus);    
                MessageBox.Show("Статус изменен!");
                DisplayZadania();
                Статус.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении статуса: " + ex.Message);
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && e.ColumnIndex == 4) 
            {
                int taskId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
                string newAnswer = dataGridView1.SelectedRows[0].Cells[e.ColumnIndex].Value.ToString();
                UpdateAnswer(taskId, newAnswer);          
                UpdateTaskStatus(taskId, "Ожидание");        
                DisplayZadania();
            }
        }
        private void UpdateAnswer(int taskId, string newAnswer)
        {
            try
            {
                con = new SqlConnection(connectionString); 
                con.Open(); 
                cmd = new SqlCommand("UPDATE Ответы SET ответ = @ответ WHERE id_задания = @id_задания", con);
                cmd.Parameters.AddWithValue("@id_задания", taskId);
                cmd.Parameters.AddWithValue("@ответ", newAnswer);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении ответа: " + ex.Message);
            }
        }      
        private void UpdateTaskStatus(int taskId, string newStatus)
        {
            try
            {
                con = new SqlConnection(connectionString); 
                con.Open(); 
                cmd = new SqlCommand("UPDATE Задания SET статус = @статус WHERE id_задания = @id_задания", con); 
                cmd.Parameters.AddWithValue("@id_задания", taskId);
                cmd.Parameters.AddWithValue("@статус", newStatus);
                cmd.ExecuteNonQuery(); 
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении статуса задания: " + ex.Message);
            }
        }
        private int GetClassIdByUserId(int userId)
        {
            try
            {
                con = new SqlConnection(connectionString);
                con.Open();

                string query = "SELECT класс_id FROM Ученики WHERE id_Пользователя = @userId";
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@userId", userId);
                dr = cmd.ExecuteReader();

                int classId = 0;
                if (dr.Read())
                {
                    classId = Convert.ToInt32(dr["класс_id"]);
                }
                dr.Close();
                con.Close();
                return classId;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении ID класса: " + ex.Message);
                return 0;
            }
        }
        private int GetUserIdByLoginAndPassword(string login, string password)
        {
            try
            {
                con = new SqlConnection(connectionString);
                con.Open();

                string query = "SELECT id_Пользователя FROM Пользователь WHERE Логин = @login AND Пароль = @password";
                cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                dr = cmd.ExecuteReader();
                int userId = 0;
                if (dr.Read())
                {
                    userId = Convert.ToInt32(dr["id_Пользователя"]);
                }
                dr.Close();
                con.Close();
                return userId;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении ID пользователя: " + ex.Message);
                return 0;
            }
        }
 
        private void HandleLogin(string login, string password)
        {
            int userId = GetUserIdByLoginAndPassword(login, password);

            if (userId > 0)
            {           
                currentUserId = userId;
                MessageBox.Show("Вход выполнен успешно!");
                // ... (Дополнительные действия, например, переход на другую форму)
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }


        private void Отчет_Click(object sender, EventArgs e)
        {
            try
            {
                con = new SqlConnection(connectionString);
                con.Open();
                string query = @"
            SELECT 
                z.id_задания,
                z.тема_урока, 
                z.описание_задания, 
                z.файл_задания, 
                z.срок_сдачи, 
                z.статус,
                o.ответ AS Ответы,
                u.ПолноеИмя AS Имя_ученика,
                c.название_класса AS Класс_ученика
            FROM 
                Задания z
            LEFT JOIN 
                Ответы o ON z.id_задания = o.id_задания
            LEFT JOIN 
                Ученики uc ON z.класс_id = uc.класс_id
            LEFT JOIN 
                Пользователь u ON uc.id_Пользователя = u.id_Пользователя
            LEFT JOIN 
                Классы c ON z.класс_id = c.id
            WHERE 1 = 1";
                if (Класс.SelectedIndex >= 0)
                {
                    query += $" AND c.название_класса = '{Класс.SelectedItem}'";
                }
                if (Ученик.SelectedIndex >= 0)
                {
                    query += $" AND u.ПолноеИмя = '{Ученик.SelectedItem}'";
                }
                cmd = new SqlCommand(query, con);
                dr = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dr.Close();
                dataGridView1.DataSource = dt;
                dataGridView1.Columns["id_задания"].Visible = false;

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            zadania form = new zadania();
            form.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            domzadania form = new domzadania();
            form.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Prosmotr form = new Prosmotr();
            form.Show();
            this.Hide();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Otchet form = new Otchet();
            form.Show();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Avtorization form = new Avtorization();
            form.Show();
            this.Hide();
        }
        private void Prosmotr_Load(object sender, EventArgs e)
        {

        }
        private void label3_Click(object sender, EventArgs e)
        {

        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Класс_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
