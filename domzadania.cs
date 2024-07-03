using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Практика
{
    public partial class domzadania : Form
    {
        private string filePath;
        private string connectionString = @"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26"";Integrated Security=True;Connect Timeout=30;";
        private SqlConnection con;
        private SqlCommand cmd;

        public domzadania()
        {
            InitializeComponent();
            DisplayZadania();
        }
        private void DisplayZadania()
        {
            try
            {
                con = new SqlConnection(connectionString);
                con.Open();
                string Query = @"
            SELECT 
                p.название_предмета AS Предмет,
                z.id_задания,
                z.тема_урока,
                z.описание_задания,
                z.статус,
                o.ответ AS Ответы, 
                o.файл_ответа AS ФайлОтвета 
            FROM
                Задания z
            LEFT JOIN
                Ответы o ON z.id_задания = o.id_задания
            LEFT JOIN
                Предметы p ON z.предмет_id = p.id";

                cmd = new SqlCommand(Query, con);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
                if (!dataGridView1.Columns.Contains("id_задания"))
                {
                    DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn();
                    idColumn.Name = "id_задания";
                    idColumn.HeaderText = "ID Задания";
                    idColumn.Visible = false;
                    dataGridView1.Columns.Add(idColumn);
                }
                else
                {
                    dataGridView1.Columns["id_задания"].Visible = false;
                }
                if (dataGridView1.Columns.Contains("ФайлОтвета"))
                {
                    dataGridView1.Columns.Remove("ФайлОтвета");
                }
                dt.Columns.Add("Имя файла", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    if (row["ФайлОтвета"] != DBNull.Value)
                    {
                        string filePath = row["ФайлОтвета"].ToString();
                        string fileName = Path.GetFileName(filePath);
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
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите задание для ответа!");
                return;
            }

            try
            {
                int taskId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["id_задания"].Value);
                string answer = Ответ.Text;
                con = new SqlConnection(connectionString);
                con.Open();
                cmd = new SqlCommand("SELECT COUNT(*) FROM Ответы WHERE id_задания = @id_задания", con);
                cmd.Parameters.AddWithValue("@id_задания", taskId);
                int existingAnswerCount = (int)cmd.ExecuteScalar();
                if (existingAnswerCount > 0)
                {                
                    cmd = new SqlCommand("UPDATE Ответы SET ответ = @ответ, файл_ответа = @файл_ответа WHERE id_задания = @id_задания", con);
                    cmd.Parameters.AddWithValue("@id_задания", taskId);
                    cmd.Parameters.AddWithValue("@ответ", answer);
                    cmd.Parameters.AddWithValue("@файл_ответа", filePath); 
                }
                else
                {                 
                    cmd = new SqlCommand("INSERT INTO Ответы (id_задания, ответ, файл_ответа) VALUES (@id_задания, @ответ, @файл_ответа)", con);
                    cmd.Parameters.AddWithValue("@id_задания", taskId);
                    cmd.Parameters.AddWithValue("@ответ", answer);
                    cmd.Parameters.AddWithValue("@файл_ответа", filePath);
                }
                cmd.ExecuteNonQuery();
                con.Close();
                UpdateTaskStatus(taskId, "Ожидание");
                MessageBox.Show("Ответ добавлен!");
                Ответ.Clear();
                DisplayZadania();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении ответа: " + ex.Message);
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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        } 
        private void Файл_TextChanged(object sender, EventArgs e)
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

        private void button4_Click_1(object sender, EventArgs e)
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
    }
}