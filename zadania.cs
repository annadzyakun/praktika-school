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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Практика
{
    public partial class zadania : Form
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader reader;
        string connectionString = @"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26"";Integrated Security=True;Connect Timeout=30;";
        public zadania()
        {
            InitializeComponent();   
            buttonSelectWordFile = new System.Windows.Forms.Button();
            buttonSelectWordFile.Text = "Выбрать Word файл";
            buttonSelectWordFile.Click += button8_Click;
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "Word Files|*.doc;*.docx";
            this.Controls.Add(buttonSelectWordFile);
        }
        private void zadania_Load(object sender, EventArgs e)
        {
            FillClassComboBox();
            FillSubjectComboBox();
            FillTeacherComboBox();
            RefreshDataGrid();
        }
        private void FillClassComboBox()
        {
            try
            {
                con = new SqlConnection(@"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26"";Integrated Security=True;Connect Timeout=30;");
                con.Open();
                cmd = new SqlCommand("SELECT название_класса FROM Классы", con);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Класс.Items.Add(reader["название_класса"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке классов: " + ex.Message);
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        private void FillSubjectComboBox()
        {
            try
            {
                con = new SqlConnection(@"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26 "";Integrated Security=True;Connect Timeout=30;");
                con.Open();
                cmd = new SqlCommand("SELECT название_предмета FROM Предметы", con);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Предмет.Items.Add(reader["название_предмета"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке предметов: " + ex.Message);
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        private void FillTeacherComboBox() 
        {
            try
            {
                con = new SqlConnection(@"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26"";Integrated Security=True;Connect Timeout=30;");
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT p.id_Пользователя, p.ПолноеИмя " +
                                                      "FROM Учителя u " + 
                                                      "JOIN Пользователь p ON u.id_Пользователя = p.id_Пользователя " +
                                                      "ORDER BY p.ПолноеИмя", con))
                {
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {                    
                        Учитель.Items.Add(new KeyValuePair<int, string>((int)reader["id_Пользователя"], reader["ПолноеИмя"].ToString()));
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке учителей: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }
        private string filePath; 
            private void button8_Click(object sender, EventArgs e)
            {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Word Documents|*.doc;*.docx";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;          
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            if (Класс.SelectedItem == null ||
                Предмет.SelectedItem == null ||
                ТемаУрока.Text == "" ||
                Описание.Text == "" ||
                Учитель.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }
            try
            {
                using (SqlConnection con = new SqlConnection(@"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26"";Integrated Security=True;Connect Timeout=30;"))
                {
                    con.Open();
                    int classId = 0;
                    string selectedClass = Класс.SelectedItem.ToString();
                    using (SqlCommand cmdGetClassId = new SqlCommand("SELECT id FROM Классы WHERE название_класса = @class", con))
                    {
                        cmdGetClassId.Parameters.AddWithValue("@class", selectedClass);
                        classId = (int)cmdGetClassId.ExecuteScalar();
                    }
                    int subjectId = 0;
                    string selectedSubject = Предмет.SelectedItem.ToString();
                    using (SqlCommand cmdGetSubjectId = new SqlCommand("SELECT id FROM Предметы WHERE название_предмета = @subject", con))
                    {
                        cmdGetSubjectId.Parameters.AddWithValue("@subject", selectedSubject);
                        subjectId = (int)cmdGetSubjectId.ExecuteScalar();
                    }
                    int userId = 0;
                    if (Учитель.SelectedItem != null)
                    {
                        dynamic selectedTeacher = Учитель.SelectedItem;
                        userId = selectedTeacher.Key;
                    }
                    else
                    {
                        MessageBox.Show("Выберите учителя!");
                        return;
                    }
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Задания (класс_id, предмет_id, id_Пользователя, тема_урока, описание_задания, файл_задания, срок_сдачи) " +
                        "VALUES (@класс_id, @предмет_id, @id_Пользователя, @тема_урока, @описание_задания, @файл_задания, @срок_сдачи)", con))
                    {
                        cmd.Parameters.AddWithValue("@класс_id", classId);
                        cmd.Parameters.AddWithValue("@предмет_id", subjectId);
                        cmd.Parameters.AddWithValue("@id_Пользователя", userId);
                        cmd.Parameters.AddWithValue("@тема_урока", ТемаУрока.Text);
                        cmd.Parameters.AddWithValue("@описание_задания", Описание.Text);
                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            byte[] fileBytes = File.ReadAllBytes(filePath);
                            cmd.Parameters.AddWithValue("@файл_ответа", fileBytes);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@файл_ответа", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@срок_сдачи", DateTime.Now); 

                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Задание добавлено успешно!");
                    Класс.SelectedIndex = -1;
                    Предмет.SelectedIndex = -1;
                    Учитель.SelectedIndex = -1;
                    ТемаУрока.Text = "";
                    Описание.Text = "";

                    Срок.Value = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении задания: " + ex.Message);
            }
            RefreshDataGrid();
        }
        private void RefreshDataGrid()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    cmd = new SqlCommand("SELECT * FROM Задания", con);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;
                    dataGridView1.Columns["файл_задания"].Visible = false;

                    dt.Columns.Add("Имя_файла", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["файл_задания"] != DBNull.Value)
                        {                           
                            string fileName = Path.GetFileName(filePath);
                            row["Имя_файла"] = fileName;
                        }
                    }
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {

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

        private void button5_Click(object sender, EventArgs e)
        {
        }

        private void Описание_TextChanged(object sender, EventArgs e)
        {

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

        private void button5_Click_1(object sender, EventArgs e)
        {
            Avtorization form = new Avtorization();
            form.Show();
            this.Hide();
        }

        private void Срок_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
