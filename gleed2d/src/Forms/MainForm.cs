﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using GLEED2D.Properties;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;
using GLEED2D.src.Utils;
using Microsoft.Xna.Framework;
using Point = System.Drawing.Point;

namespace GLEED2D
{
    public partial class MainForm : Form
    {
        public static MainForm Instance;
        String levelfilename;
        //BackgroundWorker bgw = new BackgroundWorker();


        bool dirtyflag;
        public bool DirtyFlag
        {
            get { return dirtyflag; }
            set { dirtyflag = value; updatetitlebar(); }
        }

        Cursor dragcursor;
        LinkItemsForm linkItemsForm;

        [DllImport("User32.dll")]
        private static extern int SendMessage(int Handle, int wMsg, int wParam, int lParam);

        public static void SetListViewSpacing(ListView lst, int x, int y)
        {
            SendMessage((int)lst.Handle, 0x1000 + 53, 0, y * 65536 + x);
        }


        public MainForm()
        {
            Instance = this;
            InitializeComponent();
        }
        public IntPtr getHandle()
        {
            return pictureBox1.Handle;
        }
        public void updatetitlebar()
        {
            Text = "S14 EDITOR - " + levelfilename + (DirtyFlag ? "*" : "");
        }

        public static Image getThumbNail(Bitmap bmp, int imgWidth, int imgHeight)
        {
            Bitmap retBmp = new Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format64bppPArgb);
            Graphics grp = Graphics.FromImage(retBmp);
            int tnWidth = imgWidth, tnHeight = imgHeight;
            if (bmp.Width > bmp.Height)
                tnHeight = (int)(((float)bmp.Height / (float)bmp.Width) * tnWidth);
            else if (bmp.Width < bmp.Height)
                tnWidth = (int)(((float)bmp.Width / (float)bmp.Height) * tnHeight);
            int iLeft = (imgWidth / 2) - (tnWidth / 2);
            int iTop = (imgHeight / 2) - (tnHeight / 2);
            grp.DrawImage(bmp, iLeft, iTop, tnWidth, tnHeight);
            retBmp.Tag = bmp;
            return retBmp;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //this.SetDesktopLocation(1300, 20);
            linkItemsForm = new LinkItemsForm();

            //fill zoom combobox
            for (int i = 25; i <= 200; i += 25)
            {
                zoomcombo.Items.Add(i.ToString() + "%");
            }
            zoomcombo.SelectedText = "100%";

            comboBox1.Items.Add("48x48");
            comboBox1.Items.Add("64x64");
            comboBox1.Items.Add("96x96");
            comboBox1.Items.Add("128x128");
            comboBox1.Items.Add("256x256");
            comboBox1.SelectedIndex = 1;

            SetListViewSpacing(listView2, 128 + 8, 128 + 32);

            pictureBox1.AllowDrop = true;
            // pixma tap
            comboBox3.Items.Add("48x48");
            comboBox3.Items.Add("64x64");
            comboBox3.Items.Add("96x96");
            comboBox3.Items.Add("128x128");
            //comboBox3.Items.Add("256x256");
            comboBox3.SelectedIndex = 1;

            comboBox6.Items.Add("48x48");
            comboBox6.Items.Add("64x64");
            comboBox6.Items.Add("96x96");
            comboBox6.Items.Add("128x128");
            comboBox6.SelectedIndex = 1;

        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Constants.Instance.export("settings.xml");
            Game1.Instance.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkCurrentLevelAndSaveEventually() == DialogResult.Cancel) e.Cancel = true;
        }

        //TREEVIEW
        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.N) ActionNewLayer(sender, e);
            if (e.KeyCode == Keys.Delete) ActionDelete(sender, e);
            if (e.KeyCode == Keys.F7) ActionMoveUp(sender, e);
            if (e.KeyCode == Keys.F8) ActionMoveDown(sender, e);
            if (e.KeyCode == Keys.F4) ActionCenterView(sender, e);
            if (e.KeyCode == Keys.F2) treeView1.SelectedNode.BeginEdit();
            if (e.KeyCode == Keys.D && e.Control) ActionDuplicate(sender, e);
        }
        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null) return;

            TreeNode[] nodes = treeView1.Nodes.Find(e.Label, true);
            if (nodes.Length > 0)
            {
                MessageBox.Show("A layer or item with the name \"" + e.Label + "\" already exists in the level. Please use another name!");
                e.CancelEdit = true;
                return;
            }
            if (e.Node.Tag is Level)
            {
                Level l = (Level)e.Node.Tag;
                Editor.Instance.beginCommand("Rename Level (\"" + l.Name + "\" -> \"" + e.Label + "\")");
                l.Name = e.Label;
                e.Node.Name = e.Label;
                Editor.Instance.endCommand();
            }
            if (e.Node.Tag is Layer)
            {
                Layer l = (Layer)e.Node.Tag;
                Editor.Instance.beginCommand("Rename Layer (\"" + l.Name + "\" -> \"" + e.Label + "\")");
                l.Name = e.Label;
                e.Node.Name = e.Label;
                Editor.Instance.endCommand();
            }
            if (e.Node.Tag is Item)
            {
                Item i = (Item)e.Node.Tag;
                Editor.Instance.beginCommand("Rename Item (\"" + i.Name + "\" -> \"" + e.Label + "\")");
                i.Name = e.Label;
                e.Node.Name = e.Label;
                Editor.Instance.endCommand();
            }
            propertyGrid1.Refresh();
            pictureBox1.Select();
        }
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Level)
            {
                Editor.Instance.level.Visible = e.Node.Checked;
            }
            if (e.Node.Tag is Layer)
            {
                Layer l = (Layer)e.Node.Tag;
                l.Visible = e.Node.Checked;
            }
            if (e.Node.Tag is Item)
            {
                Item i = (Item)e.Node.Tag;
                i.Visible = e.Node.Checked;
            }
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Level)
            {
                Editor.Instance.selectlevel();
            }
            if (e.Node.Tag is Layer)
            {
                Layer l = (Layer)e.Node.Tag;
                Editor.Instance.selectlayer(l);
            }
            if (e.Node.Tag is Item)
            {
                Item i = (Item)e.Node.Tag;
                Editor.Instance.selectitem(i);
            }
        }
        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
            }
        }
        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (((TreeNode)e.Item).Tag is Layer) return;
            if (((TreeNode)e.Item).Tag is Level) return;
            Editor.Instance.beginCommand("Drag Item");
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            //get source node
            TreeNode sourcenode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (sourcenode == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            else e.Effect = DragDropEffects.Move;

            //get destination node and select it
            Point p = treeView1.PointToClient(new Point(e.X, e.Y));
            TreeNode destnode = treeView1.GetNodeAt(p);
            if (destnode.Tag is Level) return;
            treeView1.SelectedNode = destnode;

            if (destnode != sourcenode)
            {
                Item i1 = (Item)sourcenode.Tag;
                if (destnode.Tag is Item)
                {
                    Item i2 = (Item)destnode.Tag;
                    Editor.Instance.moveItemToLayer(i1, i2.layer, i2);
                    int delta = 0;
                    if (destnode.Index > sourcenode.Index && i1.layer == i2.layer) delta = 1;
                    sourcenode.Remove();
                    destnode.Parent.Nodes.Insert(destnode.Index + delta, sourcenode);
                }
                if (destnode.Tag is Layer)
                {
                    Layer l2 = (Layer)destnode.Tag;
                    Editor.Instance.moveItemToLayer(i1, l2, null);
                    sourcenode.Remove();
                    destnode.Nodes.Insert(0, sourcenode);
                }
                Editor.Instance.selectitem(i1);
                Editor.Instance.draw(Game1.Instance.spriteBatch);
                Game1.Instance.GraphicsDevice.Present();
                Application.DoEvents();
            }
        }
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            Editor.Instance.endCommand();
        }



        //PICTURE BOX
        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            KeyEventArgs kea = new KeyEventArgs(e.KeyData);
            treeView1_KeyDown(sender, kea);
        }
        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            Logger.Instance.log("pictureBox1_Resize().");
            if (Game1.Instance != null) Game1.Instance.resizebackbuffer(pictureBox1.Width, pictureBox1.Height);
            if (Editor.Instance != null) Editor.Instance.camera.updateviewport(pictureBox1.Width, pictureBox1.Height);
        }
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Select();
        }
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            menuStrip1.Select();

        }
        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            ListView listview = null;
            if (tabControl1.SelectedIndex == 0)     // image
                listview = listView1;
            else if (tabControl1.SelectedIndex == 2)// frame
                listview = listView4;
            else if (tabControl1.SelectedIndex == 3)// anim
                listview = listView8;
            e.Effect = DragDropEffects.Move;
            ListViewItem lvi = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            string[] data = Regex.Split(listview.FocusedItem.Tag.ToString(), ";");
            string itemtype = data[0];
            string id = data[1];
            Editor.Instance.createTextureBrush(lvi.Name, itemtype, id);


        }
        private void pictureBox1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
            Point p = pictureBox1.PointToClient(new Point(e.X, e.Y));
            Editor.Instance.setmousepos(p.X, p.Y);
            Editor.Instance.draw(Game1.Instance.spriteBatch);
            Game1.Instance.GraphicsDevice.Present();
        }
        private void pictureBox1_DragLeave(object sender, EventArgs e)
        {
            Editor.Instance.destroyTextureBrush();
            Editor.Instance.draw(Game1.Instance.spriteBatch);
            Game1.Instance.GraphicsDevice.Present();
        }
        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            ListView listview = null;
            if (tabControl1.SelectedIndex == 0)     // image
                listview = listView1;
            else if (tabControl1.SelectedIndex == 2)// frame
                listview = listView4;
            else if (tabControl1.SelectedIndex == 3)// anim
                listview = listView8;
            Editor.Instance.paintTextureBrush(false);
            listview.Cursor = Cursors.Default;
            pictureBox1.Cursor = Cursors.Default;
        }




        // ACTIONS
        private void ActionDuplicate(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                Layer layercopy = l.clone();
                layercopy.Name = getUniqueNameBasedOn(layercopy.Name);
                for (int i = 0; i < layercopy.Items.Count; i++)
                {
                    layercopy.Items[i].Name = getUniqueNameBasedOn(layercopy.Items[i].Name);
                }
                Editor.Instance.beginCommand("Duplicate Layer \"" + l.Name + "\"");
                Editor.Instance.addLayer(layercopy);
                Editor.Instance.endCommand();
                Editor.Instance.updatetreeview();
            }
        }
        private void ActionCenterView(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Level)
            {
                Editor.Instance.camera.Position = Microsoft.Xna.Framework.Vector2.Zero;
            }
            if (treeView1.SelectedNode.Tag is Item)
            {
                Item i = (Item)treeView1.SelectedNode.Tag;
                Editor.Instance.camera.Position = i.pPosition;
            }
        }
        private void ActionRename(object sender, EventArgs e)
        {
            treeView1.SelectedNode.BeginEdit();
        }
        private void ActionNewLayer(object sender, EventArgs e)
        {
            AddLayer f = new AddLayer(this);
            f.ShowDialog();
        }
        private void ActionDelete(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                Editor.Instance.deleteLayer(l);
            }
            else if (treeView1.SelectedNode.Tag is Item)
            {
                Editor.Instance.deleteSelectedItems();
            }
        }
        private void ActionMoveUp(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                if (l.level.Layers.IndexOf(l) > 0)
                {
                    Editor.Instance.beginCommand("Move Up Layer \"" + l.Name + "\"");
                    Editor.Instance.moveLayerUp(l);
                    Editor.Instance.endCommand();
                    Editor.Instance.updatetreeview();
                }
            }
            if (treeView1.SelectedNode.Tag is Item)
            {
                Item i = (Item)treeView1.SelectedNode.Tag;
                if (i.layer.Items.IndexOf(i) > 0)
                {
                    Editor.Instance.beginCommand("Move Up Item \"" + i.Name + "\"");
                    Editor.Instance.moveItemUp(i);
                    Editor.Instance.endCommand();
                    Editor.Instance.updatetreeview();
                }
            }
        }
        private void ActionMoveDown(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                if (l.level.Layers.IndexOf(l) < l.level.Layers.Count - 1)
                {
                    Editor.Instance.beginCommand("Move Down Layer \"" + l.Name + "\"");
                    Editor.Instance.moveLayerDown(l);
                    Editor.Instance.endCommand();
                    Editor.Instance.updatetreeview();
                }
            }
            if (treeView1.SelectedNode.Tag is Item)
            {
                Item i = (Item)treeView1.SelectedNode.Tag;
                if (i.layer.Items.IndexOf(i) < i.layer.Items.Count - 1)
                {
                    Editor.Instance.beginCommand("Move Down Item \"" + i.Name + "\"");
                    Editor.Instance.moveItemDown(i);
                    Editor.Instance.endCommand();
                    Editor.Instance.updatetreeview();
                }
            }
        }
        private void ActionAddCustomProperty(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Item)
            {
                Item i = (Item)treeView1.SelectedNode.Tag;
                AddCustomProperty form = new AddCustomProperty(i.CustomProperties);
                form.ShowDialog();
            }
            if (treeView1.SelectedNode.Tag is Level)
            {
                Level l = (Level)treeView1.SelectedNode.Tag;
                AddCustomProperty form = new AddCustomProperty(l.CustomProperties);
                form.ShowDialog();
            }
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                AddCustomProperty form = new AddCustomProperty(l.CustomProperties);
                form.ShowDialog();
            }
            propertyGrid1.Refresh();
        }







        //MENU
        public void newLevel()
        {
            Application.DoEvents();
            Level newlevel = Level.createS14Template();
            newlevel.EditorRelated.Version = Editor.Instance.Version;
            Editor.Instance.loadLevel(newlevel);
            levelfilename = "untitled";
            DirtyFlag = false;
        }
        public void saveLevel(String filename)
        {
            Editor.Instance.saveLevel(filename);
            if (!Define.EXPORT_JSON)
                levelfilename = filename;
            DirtyFlag = false;

            if (Constants.Instance.SaveLevelStartApplication)
            {
                if (!File.Exists(Constants.Instance.SaveLevelApplicationToStart))
                {
                    MessageBox.Show("The file \"" + Constants.Instance.SaveLevelApplicationToStart + "\" doesn't exist!\nPlease provide a valid application executable in Tools -> Settings -> Save Level!\nLevel was saved.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (Constants.Instance.SaveLevelAppendLevelFilename)
                {
                    Process.Start(Constants.Instance.SaveLevelApplicationToStart, "\"" + levelfilename + "\"");
                }
                else
                {
                    Process.Start(Constants.Instance.SaveLevelApplicationToStart);
                }
            }

        }
        public void loadLevel(String filename)
        {
            Logger.Instance.log("Load level: " + filename);
            string data_path = Directory.GetParent(filename).ToString() + "\\data";
            Define.file_data_path = !Directory.Exists(data_path) ? String.Empty : data_path;

            Level level = Level.FromFile(filename, Game1.Instance.Content);

            if (level.EditorRelated.Version == null || level.EditorRelated.Version.CompareTo("1.0") < 0)
            {
                DialogResult dr = MessageBox.Show(
                    "This file was created with a version of GLEED2D less than 1.3.\n" +
                    "In version 1.3 the datatype of 'Scale' changed from 'float' to 'Vector2'.\n" +
                    "The file you tried to open should therefore be converted accordingly.\n" +
                    "GLEED2D can do that automatically for you.\n\n"+
                    "(Basically, all that's done is convert \n"+
                    "<Scale>1.234</Scale> to \n"+
                    "<Scale><X>1.234</X><Y>1.234</Y></Scale>)\n\n" +
                    "Do you want the file to be converted before opening?",
                    "Convert?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.OK)
                {
                    //Convert the file, save it and open it 
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filename);
                    //change Scale from float to Vector2 with X = Y = previous scale factor
                    doc.InnerXml = Regex.Replace(doc.InnerXml, @"<Scale>(\d+\.?\d*)", "<Scale><X>$1</X><Y>$1</Y>");

                    //update/insert <Version> tag
                    if (doc.GetElementsByTagName("Version").Count > 0) //<Version> tag exists and must be updated
                    {
                        doc.InnerXml = Regex.Replace(doc.InnerXml, @"<Version>(\d+\.?\d*\.?\d*\.?\d*)", "<Version>" + Editor.Instance.Version);
                    }
                    else //level files prior to 1.3 didn't have a version tag so insert one
                    {
                        doc.GetElementsByTagName("EditorRelated").Item(0).InnerXml += "<Version>" + Editor.Instance.Version + "</Version>";
                    }
                    //save the changes...
                    doc.Save(filename);

                    //...and load it again
                    level = Level.FromFile(filename, Game1.Instance.Content);
                }
                else
                {
                    return;
                }
            }


            Editor.Instance.loadLevel(level);
            levelfilename = filename;
            DirtyFlag = false;
        }
        public DialogResult checkCurrentLevelAndSaveEventually()
        {
            if (DirtyFlag)
            {
                DialogResult dr = MessageBox.Show("The current level has not been saved. Do you want to save now?", "Save?",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    if (levelfilename == "untitled")
                    {
                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.Filter = "XML Files (*.s14map)|*.s14map";
                        if (dialog.ShowDialog() == DialogResult.OK) saveLevel(dialog.FileName);
                        else return DialogResult.Cancel;
                    }
                    else
                    {
                        saveLevel(levelfilename);
                    }
                }
                if (dr == DialogResult.Cancel) return DialogResult.Cancel;
            }
            return DialogResult.OK;
        }

        private void FileNew(object sender, EventArgs e)
        {
            if (checkCurrentLevelAndSaveEventually() == DialogResult.Cancel) return;
            newLevel();
        }
        private void FileOpen(object sender, EventArgs e)
        {
            if (checkCurrentLevelAndSaveEventually() == DialogResult.Cancel) return;
            OpenFileDialog opendialog = new OpenFileDialog();
            opendialog.Filter = "S14MAP Files (*.s14map)|*.s14map";
            if (opendialog.ShowDialog() == DialogResult.OK) loadLevel(opendialog.FileName);
        }
        private void FileSave(object sender, EventArgs e)
        {
            if (levelfilename == "untitled") FileSaveAs(sender, e);
            else saveLevel(levelfilename);

        }
        private void FileSaveAs(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "S14MAP Files (*.s14map)|*.s14map";
            if (dialog.ShowDialog() == DialogResult.OK) saveLevel(dialog.FileName);
        }
        private void FileExit(object sender, EventArgs e)
        {
            Close();
        }



        private void EditUndo(object sender, EventArgs e)
        {
            Editor.Instance.undo();
        }

        private void EditRedo(object sender, EventArgs e)
        {
            Editor.Instance.redo();
        }

        private void EditSelectAll(object sender, EventArgs e)
        {
            Editor.Instance.selectAll();
        }

        private void zoomcombo_TextChanged(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = zoomcombo.SelectedText;
            if (zoomcombo.Text.Length > 0 && Editor.Instance != null)
            {
                float zoom = float.Parse(zoomcombo.Text.Substring(0, zoomcombo.Text.Length - 1));
                Editor.Instance.camera.Scale = zoom / 100;
            }
        }

        private void HelpQuickGuide(object sender, EventArgs e)
        {
            new QuickGuide().Show();
        }

        private void HelpAbout(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog();
        }


        private void ToolsMenu_MouseEnter(object sender, EventArgs e)
        {
            moveSelectedItemsToLayerToolStripMenuItem.Enabled =
            copySelectedItemsToLayerToolStripMenuItem.Enabled = Editor.Instance.SelectedItems.Count > 0;
            alignHorizontallyToolStripMenuItem.Enabled =
            alignVerticallyToolStripMenuItem.Enabled =
            alignRotationToolStripMenuItem.Enabled =
            alignScaleToolStripMenuItem.Enabled = Editor.Instance.SelectedItems.Count > 1;

            linkItemsByACustomPropertyToolStripMenuItem.Enabled = Editor.Instance.SelectedItems.Count == 2;

        }
        private void ToolsMenu_Click(object sender, EventArgs e)
        {
        }
        private void ToolsMoveToLayer(object sender, EventArgs e)
        {
            LayerSelectForm f = new LayerSelectForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                Layer chosenlayer = (Layer)f.treeView1.SelectedNode.Tag;
                Editor.Instance.moveSelectedItemsToLayer(chosenlayer);
            }

        }
        private void ToolsCopyToLayer(object sender, EventArgs e)
        {
            LayerSelectForm f = new LayerSelectForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                Layer chosenlayer = (Layer)f.treeView1.SelectedNode.Tag;
                Editor.Instance.copySelectedItemsToLayer(chosenlayer);
            }
        }
        private void ToolsLinkItems(object sender, EventArgs e)
        {
            linkItemsForm.ShowDialog();
        }
        private void ToolsAlignHorizontally(object sender, EventArgs e)
        {
            Editor.Instance.alignHorizontally();
        }
        private void ToolsAlignVertically(object sender, EventArgs e)
        {
            Editor.Instance.alignVertically();
        }
        private void ToolsAlignRotation(object sender, EventArgs e)
        {
            Editor.Instance.alignRotation();
        }
        private void ToolsAlignScale(object sender, EventArgs e)
        {
            Editor.Instance.alignScale();
        }
        private void ToolsSettings(object sender, EventArgs e)
        {
            SettingsForm f = new SettingsForm();
            f.ShowDialog();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            Editor.Instance.draw(Game1.Instance.spriteBatch);
            Game1.Instance.GraphicsDevice.Present();
            Application.DoEvents();
        }


        private void propertyGrid1_Enter(object sender, EventArgs e)
        {
            Editor.Instance.beginCommand("Edit in PropertyGrid");
        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Editor.Instance.endCommand();
            Editor.Instance.beginCommand("Edit in PropertyGrid");
        }

        public void UndoManyCommands(object sender, ToolStripItemClickedEventArgs e)
        {
            Command c = (Command)e.ClickedItem.Tag;
            Editor.Instance.undoMany(c);
        }

        private void RedoManyCommands(object sender, ToolStripItemClickedEventArgs e)
        {
            Command c = (Command)e.ClickedItem.Tag;
            Editor.Instance.redoMany(c);
        }






        private void comboSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    listView1.LargeImageList = imageList48;
                    SetListViewSpacing(listView1, 48 + 8, 48 + 32);
                    break;
                case 1:
                    listView1.LargeImageList = imageList64;
                    SetListViewSpacing(listView1, 64 + 8, 64 + 32);
                    break;
                case 2:
                    listView1.LargeImageList = imageList96;
                    SetListViewSpacing(listView1, 96 + 8, 96 + 32);
                    break;
                case 3:
                    listView1.LargeImageList = imageList128;
                    SetListViewSpacing(listView1, 128 + 8, 128 + 32);
                    break;
                case 4:
                    listView1.LargeImageList = imageList256;
                    SetListViewSpacing(listView1, 256 + 8, 256 + 32);
                    break;
            }
        }

        private void buttonFolderUp_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = Directory.GetParent(textBox1.Text);
            if (di == null) return;
            loadfolder(di.FullName);
            /*textBox1.Text = Directory.GetParent(textBox1.Text).FullName;
            loadfolder(textBox1.Text);*/
        }
        private void chooseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            d.SelectedPath = textBox1.Text;
            if (d.ShowDialog() == DialogResult.OK) loadfolder(d.SelectedPath);
        }
        private void listView1_Click(object sender, EventArgs e)
        {
            ListView listview = (ListView)sender;
            toolStripStatusLabel1.Text = listview.FocusedItem.ToolTipText;
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string[] data = Regex.Split(listView1.FocusedItem.Tag.ToString(), ";");
            string itemtype = data[0];
            string id = data[1];
            if (itemtype == Define.TYPE_FOLDER)
            {
                loadfolder(listView1.FocusedItem.Name);
            }
            else if (itemtype == Define.TYPE_IMAGE)
            {
                Editor.Instance.createTextureBrush(listView1.FocusedItem.Name, Define.TYPE_IMAGE, id);
            }
        }
        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListView listview = (ListView) sender;


            ListViewItem lvi = (ListViewItem)e.Item;
            string[] data = Regex.Split(lvi.Tag.ToString(), ";");
            string itemtype = data[0];
            string id = data[1];
            if (itemtype == Define.TYPE_FOLDER ) return;
            toolStripStatusLabel1.Text = lvi.ToolTipText;

            Bitmap bmp = new Bitmap(listview.LargeImageList.Images[lvi.ImageKey]);
            dragcursor = new Cursor(bmp.GetHicon());
            listview.DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void listView1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        private void listView1_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            ListView listview = (ListView)sender;
            if (e.Effect == DragDropEffects.Move)
            {
                e.UseDefaultCursors = false;
                listview.Cursor = dragcursor;
                pictureBox1.Cursor = Cursors.Default;
            }
            else
            {
                e.UseDefaultCursors = true;
                listview.Cursor = Cursors.Default;
                pictureBox1.Cursor = Cursors.Default;
            }
        }
        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            ListView listview = (ListView)sender;
            listview.Cursor = Cursors.Default;
            pictureBox1.Cursor = Cursors.Default;
        }

        public string getUniqueNameBasedOn(string name)
        {
            int i=0;
            string newname = "Copy of " + name;
            while (treeView1.Nodes.Find(newname, true).Length>0) 
            {
                newname = "Copy(" + i++.ToString() + ") of " + name;
            }
            return newname;
        }




        public void loadfolder(string path)
        {
            //loadfolder_background(path);
            loadfolder_foreground(path);
        }


        public void loadfolder_foreground(string path)
        {
            imageList48.Images.Clear();
            imageList64.Images.Clear();
            imageList96.Images.Clear();
            imageList128.Images.Clear();
            imageList256.Images.Clear();

            Image img = Resources.folder;
            imageList48.Images.Add(img);
            imageList64.Images.Add(img);
            imageList96.Images.Add(img);
            imageList128.Images.Add(img);
            imageList256.Images.Add(img);

            listView1.Clear();

            DirectoryInfo di = new DirectoryInfo(path);
            textBox1.Text = di.FullName;
            DirectoryInfo[] folders = di.GetDirectories();
            foreach (DirectoryInfo folder in folders)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = folder.Name;
                lvi.ToolTipText = folder.Name;
                lvi.ImageIndex = 0;
                lvi.Tag = Define.TYPE_FOLDER + Define.TYPE_SEPARATE + "0"; ;
                lvi.Name = folder.FullName;
                listView1.Items.Add(lvi);
            }

            string filters = "*.jpg;*.png;*.bmp;";
            List<FileInfo> fileList = new List<FileInfo>();
            string[] extensions = filters.Split(';');
            foreach (string filter in extensions) fileList.AddRange(di.GetFiles(filter));
            FileInfo[] files = fileList.ToArray();
            
            foreach (FileInfo file in files)
            {
                Bitmap bmp = new Bitmap(file.FullName);
                imageList48.Images.Add(file.FullName, getThumbNail(bmp, 48, 48));
                imageList64.Images.Add(file.FullName, getThumbNail(bmp, 64, 64));
                imageList96.Images.Add(file.FullName, getThumbNail(bmp, 96, 96));
                imageList128.Images.Add(file.FullName, getThumbNail(bmp, 128, 128));
                imageList256.Images.Add(file.FullName, getThumbNail(bmp, 256, 256));

                ListViewItem lvi = new ListViewItem();
                lvi.Name = file.FullName;
                lvi.Text = file.Name;
                lvi.ImageKey = file.FullName;
                lvi.Tag = Define.TYPE_IMAGE + Define.TYPE_SEPARATE + "0";
                lvi.ToolTipText = file.Name + " (" + bmp.Width.ToString() + " x " + bmp.Height.ToString() + ")";

                listView1.Items.Add(lvi);
            }
        }






        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView2.FocusedItem.Text == "Rectangle")
            {
                Editor.Instance.createPrimitiveBrush(PrimitiveType.Rectangle);
            }
            if (listView2.FocusedItem.Text == "Circle")
            {
                Editor.Instance.createPrimitiveBrush(PrimitiveType.Circle);
            }
            if (listView2.FocusedItem.Text == "Path")
            {
                Editor.Instance.createPrimitiveBrush(PrimitiveType.Path);
            }

        }



        private void RunLevel(object sender, EventArgs e)
        {
            if (Constants.Instance.RunLevelStartApplication)
            {
                if (!System.IO.File.Exists(Constants.Instance.RunLevelApplicationToStart))
                {
                    MessageBox.Show("The file \"" + Constants.Instance.RunLevelApplicationToStart + "\" doesn't exist!\nPlease provide a valid application executable in Tools -> Settings -> Run Level!",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                FileSave(sender, e);
                if (Constants.Instance.RunLevelAppendLevelFilename)
                {
                    System.Diagnostics.Process.Start(Constants.Instance.RunLevelApplicationToStart, "\"" + levelfilename + "\"");
                }
                else
                {
                    System.Diagnostics.Process.Start(Constants.Instance.RunLevelApplicationToStart);
                }
            }
        }

        private void CustomPropertyContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (propertyGrid1.SelectedGridItem.Parent.Label != "Custom Properties") e.Cancel = true;
        }

        private void deleteCustomProperty(object sender, EventArgs e)
        {
            Editor.Instance.beginCommand("Delete Custom Property");
            DictionaryPropertyDescriptor dpd = (DictionaryPropertyDescriptor)propertyGrid1.SelectedGridItem.PropertyDescriptor;
            dpd.sdict.Remove(dpd.Name);
            propertyGrid1.Refresh();
            Editor.Instance.endCommand();
        }

        private void ViewGrid_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowGrid = ShowGridButton.Checked = ViewGrid.Checked;
        }
        
        private void ViewWorldOrigin_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowWorldOrigin = ShowWorldOriginButton.Checked = ViewWorldOrigin.Checked;
        }
        
        private void ShowGridButton_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowGrid = ViewGrid.Checked = ShowGridButton.Checked;
        }

        private void ShowWorldOriginButton_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowWorldOrigin = ViewWorldOrigin.Checked = ShowWorldOriginButton.Checked;
        }

        private void SnapToGridButton_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.SnapToGrid =  ViewSnapToGrid.Checked = SnapToGridButton.Checked;
        }

        private void ViewSnapToGrid_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.SnapToGrid = SnapToGridButton.Checked = ViewSnapToGrid.Checked;
        }

        private void isoCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isoCombo.Text.Length > 0 && Editor.Instance != null)
            {
                if (isoCombo.Text == Define.TWO_D)
                    Define.is_iso = false;
                else
                    Define.is_iso = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            d.SelectedPath = pixma_path.Text;
            if (d.ShowDialog() == DialogResult.OK) loadPixmaFrames(d.SelectedPath);
        }

        public void loadPixmaFrames(string path)
        {
            framesList48.Images.Clear();
            framesList64.Images.Clear();
            framesList96.Images.Clear();
            framesList128.Images.Clear();
            framesList256.Images.Clear();

            Image img = Resources.folder;
            framesList48.Images.Add(img);
            framesList64.Images.Add(img);
            framesList96.Images.Add(img);
            framesList128.Images.Add(img);
            framesList256.Images.Add(img);

            listView4.Clear();

            DirectoryInfo di = new DirectoryInfo(path);
            pixma_path.Text = di.FullName;
            DirectoryInfo[] folders = di.GetDirectories();
            foreach (DirectoryInfo folder in folders)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = folder.Name;
                lvi.ToolTipText = folder.Name;
                lvi.ImageIndex = 0;
                lvi.Tag = Define.TYPE_FOLDER + Define.TYPE_SEPARATE + "0";
                lvi.Name = folder.FullName;
                listView4.Items.Add(lvi);
            }

            string filters = "*.anim";
            List<FileInfo> fileList = new List<FileInfo>();
            string[] extensions = filters.Split(';');
            foreach (string filter in extensions) fileList.AddRange(di.GetFiles(filter));
            FileInfo[] files = fileList.ToArray();
            Pixma pixma;
            foreach (FileInfo file in files)
            {
                pixma = PixmaManager.getCache(file.FullName);
                if (pixma == null)
                {
                    pixma = new Pixma(file.Name);
                    pixma.load(file.FullName);
                    PixmaManager.cache(file.FullName, pixma);
                }

                int i = 0, length = pixma.NumFrame;
                for (i = 0; i < length; i++)
                {
                    if (pixma.hasFrameName(i))
                    {
                        PixFrame frame = new PixFrame(i, file.FullName, Vector2.Zero);
                        Bitmap bmp = frame.getBitmapView();
                        framesList48.Images.Add(frame.FrameName + file.FullName, getThumbNail(bmp, 48, 48));
                        framesList64.Images.Add(frame.FrameName + file.FullName, getThumbNail(bmp, 64, 64));
                        framesList96.Images.Add(frame.FrameName + file.FullName, getThumbNail(bmp, 96, 96));
                        framesList128.Images.Add(frame.FrameName + file.FullName, getThumbNail(bmp, 128, 128));

                        ListViewItem lvi = new ListViewItem();
                        lvi.Name = file.FullName;
                        lvi.Text = frame.FrameName;
                        lvi.ImageKey = frame.FrameName + file.FullName;
                        lvi.Tag = Define.TYPE_FRAME + Define.TYPE_SEPARATE + i;
                        lvi.ToolTipText = frame.FrameName + " - " + file.Name + " (" + bmp.Width.ToString() + " x " + bmp.Height.ToString() + ")";

                        listView4.Items.Add(lvi);
                    }
                }

            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox3.SelectedIndex)
            {
                case 0:
                    listView4.LargeImageList = framesList48;
                    SetListViewSpacing(listView4, 48 + 8, 48 + 32);
                    break;
                case 1:
                    listView4.LargeImageList = framesList64;
                    SetListViewSpacing(listView4, 64 + 8, 64 + 32);
                    break;
                case 2:
                    listView4.LargeImageList = framesList96;
                    SetListViewSpacing(listView4, 96 + 8, 96 + 32);
                    break;
                case 3:
                    listView4.LargeImageList = framesList128;
                    SetListViewSpacing(listView4, 128 + 8, 128 + 32);
                    break;
            }
        }

        private void listView4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string[] data = Regex.Split(listView4.FocusedItem.Tag.ToString(), ";");
            string itemtype = data[0];
            string id = data[1];
            if (itemtype == Define.TYPE_FOLDER)
            {
                loadPixmaFrames(listView4.FocusedItem.Name);
            }
            else if (itemtype == Define.TYPE_FRAME)
            {
                Editor.Instance.createTextureBrush(listView4.FocusedItem.Name, Define.TYPE_FRAME, id);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = Directory.GetParent(pixma_path.Text);
            if (di == null) return;
            loadPixmaFrames(di.FullName);
        }


        private void toolStripButton4_CheckedChanged(object sender, EventArgs e)
        {
            Define.is_nagetive = !toolStripButton4.Checked;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = Directory.GetParent(textBox5.Text);
            if (di == null) return;
            loadPixmaAnims(di.FullName);
        }

        public void loadPixmaAnims(string path)
        {
            animList48.Images.Clear();
            animList64.Images.Clear();
            animList96.Images.Clear();
            animList128.Images.Clear();
            animList256.Images.Clear();

            Image img = Resources.folder;
            animList48.Images.Add(img);
            animList64.Images.Add(img);
            animList96.Images.Add(img);
            animList128.Images.Add(img);
            animList256.Images.Add(img);

            listView8.Clear();

            DirectoryInfo di = new DirectoryInfo(path);
            textBox5.Text = di.FullName;
            DirectoryInfo[] folders = di.GetDirectories();
            foreach (DirectoryInfo folder in folders)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = folder.Name;
                lvi.ToolTipText = folder.Name;
                lvi.ImageIndex = 0;
                lvi.Tag = Define.TYPE_FOLDER + Define.TYPE_SEPARATE + "0";
                lvi.Name = folder.FullName;
                listView8.Items.Add(lvi);
            }

            string filters = "*.anim";
            List<FileInfo> fileList = new List<FileInfo>();
            string[] extensions = filters.Split(';');
            foreach (string filter in extensions) fileList.AddRange(di.GetFiles(filter));
            FileInfo[] files = fileList.ToArray();
            Pixma pixma;
            foreach (FileInfo file in files)
            {
                pixma = PixmaManager.getCache(file.FullName);
                if (pixma == null)
                {
                    pixma = new Pixma(file.Name);
                    pixma.load(file.FullName);
                    PixmaManager.cache(file.FullName, pixma);
                }

                int i = 0, length = pixma.NumAnim;
                for (i = 0; i < length; i++)
                {
                    if ( pixma.hasAnimName(i))
                    {
                        Bitmap bmp = pixma.GetAnimIcon(i);
                        string animName = pixma.GetAnimName(i);
                        animList48.Images.Add(animName + file.FullName, getThumbNail(bmp, 48, 48));
                        animList64.Images.Add(animName + file.FullName, getThumbNail(bmp, 64, 64));
                        animList96.Images.Add(animName + file.FullName, getThumbNail(bmp, 96, 96));
                        animList128.Images.Add(animName + file.FullName, getThumbNail(bmp, 128, 128));

                        ListViewItem lvi = new ListViewItem();
                        lvi.Name = file.FullName;
                        lvi.Text = animName;
                        lvi.ImageKey = animName + file.FullName;
                        lvi.Tag = Define.TYPE_ANIM + Define.TYPE_SEPARATE + i;
                        lvi.ToolTipText = animName + " - " + file.Name + " (" + bmp.Width.ToString() + " x " + bmp.Height.ToString() + ")";

                        listView8.Items.Add(lvi);
                    }
                }

            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            d.SelectedPath = textBox5.Text;
            if (d.ShowDialog() == DialogResult.OK) loadPixmaAnims(d.SelectedPath);
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox6.SelectedIndex)
            {
                case 0:
                    listView8.LargeImageList = animList48;
                    SetListViewSpacing(listView8, 48 + 8, 48 + 32);
                    break;
                case 1:
                    listView8.LargeImageList = animList64;
                    SetListViewSpacing(listView8, 64 + 8, 64 + 32);
                    break;
                case 2:
                    listView8.LargeImageList = animList96;
                    SetListViewSpacing(listView8, 96 + 8, 96 + 32);
                    break;
                case 3:
                    listView8.LargeImageList = animList128;
                    SetListViewSpacing(listView8, 128 + 8, 128 + 32);
                    break;
            }
        }

        private void listView8_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string[] data = Regex.Split(listView8.FocusedItem.Tag.ToString(), ";");
            string itemtype = data[0];
            string id = data[1];
            if (itemtype == Define.TYPE_FOLDER)
            {
                loadPixmaAnims(listView8.FocusedItem.Name);
            }
            else if (itemtype == Define.TYPE_ANIM)
            {
                Editor.Instance.createTextureBrush(listView8.FocusedItem.Name, Define.TYPE_ANIM, id);
            }

        }

        private void toolStripButton2_CheckedChanged(object sender, EventArgs e)
        {
            Game1.Instance.jugger.Play =  toolStripButton2.Checked;
        }

        private void zoder_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Layer)
            {
                Layer l = (Layer)treeView1.SelectedNode.Tag;
                l.Items = Utils.zoder(l.Items);
                Editor.Instance.updatetreeview();
            }
        }

        public void checkRunFromFile()
        {
            if (Define.run_from_file.Length > 0)
            {
                Logger.Instance.log("checkRunFromFile: success!");
                loadLevel(Define.run_from_file);
            }
        }

        private void exportJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Define.EXPORT_JSON = true;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = Editor.Instance.level.Name;
            dialog.Filter = "S14JSON Files (*.s14json)|*.s14json";
            if (dialog.ShowDialog() == DialogResult.OK) saveLevel(dialog.FileName);

            Define.EXPORT_JSON = false;
        }

        private void openLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Instance.open();
        }





    }
}
