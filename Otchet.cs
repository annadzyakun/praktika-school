using System;
using System.Collections;
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

namespace Практика
{
    public partial class Otchet : Form
    {
        SqlConnection con; 
        SqlCommand cmd; 
        SqlDataReader dr; 
        string connectionString = @"Data Source=.;Initial Catalog=""Практика МОУ СОШ 26"";Integrated Security=True;Connect Timeout=30;";
        public Otchet()
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
        private void ExportToWord(DataTable dataTable)
        {
            if (dataTable.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
                word.Application.Documents.Add(Type.Missing);
                object collapseEnd = Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd;
                word.Application.ActiveDocument.Content.Collapse(ref collapseEnd);
                Microsoft.Office.Interop.Word.Table table = word.Application.ActiveDocument.Tables.Add(
                    word.Application.ActiveDocument.Content,
                    dataTable.Rows.Count + 1,
                    dataTable.Columns.Count,
                    Type.Missing,
                    Type.Missing);
                table.Borders.OutsideLineStyle = Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;
                table.Borders.InsideLineStyle = Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;

                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    table.Cell(1, i + 1).Range.Text = dataTable.Columns[i].ColumnName;
                }

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        table.Cell(i + 2, j + 1).Range.Text = dataTable.Rows[i][j].ToString();
                    }
                }
                word.Visible = true;
            }
            else
            {
                MessageBox.Show("Нет данных для экспорта!");
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            var query = @"
            SELECT 
                z.id_задания, 
                z.тема_урока, 
                z.описание_задания, 
                z.файл_задания, 
                z.срок_сдачи, 
                z.статус,
                o.ответ AS Ответы,
                u.ПолноеИмя AS Имя_ученика,
                p.название_предмета AS Предмет,
                c.название_класса AS Класс_ученика  -- Добавлено
            FROM 
                Задания z
            LEFT JOIN 
                Ответы o ON z.id_задания = o.id_задания
            LEFT JOIN 
                Ученики uc ON z.класс_id = uc.класс_id
            LEFT JOIN 
                Пользователь u ON uc.id_Пользователя = u.id_Пользователя
            LEFT JOIN
                Предметы p ON z.предмет_id = p.id
            LEFT JOIN 
                Классы c ON uc.класс_id = c.id"; 
            var datatable = new DataTable();
            queryReturnData(query, datatable);
            ExportToWord(datatable);
        }
        public DataTable queryReturnData(string query, DataTable dataTable)
        {
            SqlConnection myCon = new SqlConnection(connectionString);
            myCon.Open();

            SqlDataAdapter SDA = new SqlDataAdapter(query, myCon);
            SDA.SelectCommand.ExecuteNonQuery();

            SDA.Fill(dataTable);
            return dataTable;
        }
    

        private void button4_Click(object sender, EventArgs e)
        {
            Prosmotr f= new Prosmotr();
            f.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            domzadania f = new domzadania();
            f.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            zadania f = new zadania();
            f.Show();
            this.Hide();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Otchet form = new Otchet();
            form.Show();
            this.Hide();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Prosmotr form = new Prosmotr();
            form.Show();
            this.Hide();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            domzadania form = new domzadania();
            form.Show();
            this.Hide();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            zadania form = new zadania();
            form.Show();
            this.Hide();
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            Avtorization form = new Avtorization();
            form.Show();
            this.Hide();
        }

        private void Предмет_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {


        }
        private void Класс_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }
        private void Статус_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}