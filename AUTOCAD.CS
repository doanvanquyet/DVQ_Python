// (C) Copyright 2020 by  
//
using System;
using System.Windows.Forms;
using System.IO;
using System.Text;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Windows;


using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Collections.Generic;
using System.Linq;



// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(AutoCAD_CSharp_plug_in3.MyCommands))]

namespace AutoCAD_CSharp_plug_in3
{
    public struct SettingData
    {
        public string ygName;
        public string ygType;
        public string ygWname;
        public double wideLen;
        public string ygDname;
        public double depthLen1;
        public double depthLen2;
        public string ygHname;
        public string lmrKey;

        public SettingData(string ygName, string ygType, string ygWname, double wideLen, string ygDname, double depthLen1, double depthLen2, string ygHname, string lmrKey)
        {
            this.ygName = ygName;
            this.ygType = ygType;
            this.ygWname = ygWname;
            this.wideLen = wideLen;
            this.ygDname = ygDname;
            this.depthLen1 = depthLen1;
            this.depthLen2 = depthLen2;
            this.ygHname = ygHname;
            this.lmrKey = lmrKey;
        }
    }

    public struct DepthData
    {
        public string dKeyName;
        public double dLen1;
        public double dLen2;

        public DepthData(string dKeyName,double dLen1,double dLen2)
            {
            this.dKeyName = dKeyName;
            this.dLen1 = dLen1;
            this.dLen2 = dLen2;
            }
    }

    public struct WideData
    {
        public string wKeyName;
        public double wLen;

        public WideData(string wKeyName, double wLen)
        {
            this.wKeyName = wKeyName;
            this.wLen = wLen;
        }
    }

    public struct NewGridx
    {
        public string gNameX;
        public double gDistanceX;

        public NewGridx(string gNameX, double gDistanceX)
        {
            this.gNameX = gNameX;
            this.gDistanceX = gDistanceX;
        }
    }
    public struct NewGridy
    {
        public string gNameY;
        public double gDistanceY;

        public NewGridy(string gNameY, double gDistanceY)
        {
            this.gNameY = gNameY;
            this.gDistanceY = gDistanceY;
        }
    }

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        // Modal Command with localized name
        [CommandMethod("MyGroup", "MyCommand", "MyCommandLocal", CommandFlags.Modal)]
        public void MyCommand() // This method can have any name
        {

            // CSV  -----------------------------------------------------------------


            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            appPath = appPath.Replace("AutoCAD CSharp plug-in3.dll", "YGparam");
            string csvPath = appPath + "\\Depth.csv";

            List<DepthData> listDepth = new List<DepthData>();

            using (StreamReader reader = new StreamReader(csvPath))
            {
                while (!reader.EndOfStream)
                {
                    string l = reader.ReadLine();
                    string[] ary = l.Split(',');

                    string depthName = ary[0];
                    double depthLen1 = double.Parse(ary[1]); 
                    double depthLen2 = double.Parse(ary[2]);

                    listDepth.Add(new DepthData(depthName, depthLen1, depthLen2));
                }
            }


            csvPath = appPath + "\\VGC.csv";
            List<WideData> listVGCwide = new List<WideData>();

            using (StreamReader reader = new StreamReader(csvPath))
            {
                while (!reader.EndOfStream)
                {
                    string l = reader.ReadLine();
                    string[] ary = l.Split(',');

                    string wideName = ary[0];
                    double wideLen = double.Parse(ary[1]);

                    listVGCwide.Add(new WideData(wideName, wideLen));
                }
            }

            csvPath = appPath + "\\VKC.csv";
            List<WideData> listVKCwide = new List<WideData>();

            using (StreamReader reader = new StreamReader(csvPath))
            {
                while (!reader.EndOfStream)
                {
                    string l = reader.ReadLine();
                    string[] ary = l.Split(',');

                    string wideName = ary[0];
                    double wideLen = double.Parse(ary[1]);

                    listVKCwide.Add(new WideData(wideName, wideLen));
                }
            }


            // Select File TXT //-----------------------------------------------------
            string txtFileName = "";

            OpenFileDialog ofDialog = new OpenFileDialog();

            //Def Folder
            ofDialog.InitialDirectory = @"C:work";
            ofDialog.Filter = "txt files (*.txt)|*.txt";

            //Title
            ofDialog.Title = "TXT Load";

            //Show
            if (ofDialog.ShowDialog() == DialogResult.OK)
            {

                txtFileName = ofDialog.FileName;
            }
            else
            {
                txtFileName = "";
                return;
            }

            // オブジェクトを破棄する
            ofDialog.Dispose();


            //Read Line 
            List<string> lines = File.ReadAllLines(txtFileName, Encoding.GetEncoding("shift_jis")).ToList();

            //[MgNO] YYG201027-1 
            //[BNAME] 伊藤真由美様邸
            //[DNAME] VGC-3059H+VKC-2859H+VGC-2659H
            // [Span] 3  [3] = "[Span]3"
            //[Type]  S
            //[Depth] 59
            //[High]  H
            //[DrawingName] VGC-3059H VKC-2859H VGC-2659H 3連棟

            string mgNo = lines[0].Replace("[MgNO]", "");

            string bName = lines[1].Replace("[BNAME]", "");
            string dName = lines[2].Replace("[DNAME]", "");
            int spanInt = int.Parse(lines[3].Replace( "[Span]",""));
            string typeStd = lines[4].Replace("[Type]", "");
            string depthKey = lines[5].Replace("[Depth]", "");
            string highKey = lines[6].Replace("[High]", "");
            string drawName = lines[7].Replace("[DrawingName]", "");


            List<SettingData> listG = new List<SettingData>();

            for (int i = 0; i < spanInt; i++)
            {
                string ygName;
                string ygType;
                string ygWname;
                double wideLen = 0;
                string ygDname;
                double depthLen1 =0;
                double depthLen2 =0;
                string ygHname;
                string lmrKey;

                //[T0]VGC-3059H
                string target = "]";
                ygName = lines[8 + i];
                int num = ygName.IndexOf(target)+1;
                ygName = ygName.Substring(num);

                ygType = typeStd;

                ygWname = ygName;
                target = "-";
                num = ygWname.IndexOf(target)+1;
                ygWname = ygWname.Substring(num,2);

                string keyTypeName = ygName.Substring(0, 3);

                if (keyTypeName == "VKC")
                {
                    foreach (WideData wlistDin  in listVKCwide)
                    {
                        if (ygWname == wlistDin.wKeyName)
                        {
                            wideLen = wlistDin.wLen;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (WideData wlistDin in listVGCwide)
                    {
                        if (ygWname == wlistDin.wKeyName)
                        {
                            wideLen = wlistDin.wLen;
                            break;
                        }
                    }
                }


                ygDname = depthKey;
                foreach (DepthData dlistDin in listDepth)
                {
                    if (ygDname == dlistDin.dKeyName)
                    {
                        depthLen1 = dlistDin.dLen1;
                        depthLen2 = dlistDin.dLen2;
                        break;
                    }
                }

                ygHname = highKey;

                //lmrKey
                if (i==0)
                { lmrKey = "L"; }
                else if (i== spanInt - 1)
                { lmrKey = "R"; }
                else
                { lmrKey = "M"; }

                listG.Add(new SettingData(ygName, ygType, ygWname, wideLen, ygDname ,depthLen1 , depthLen2, ygHname, lmrKey));

            }

            // -----------------------------------------------
            //   Wide ※88は33＋50
            //                      3268.5
            //                      5488.5
            // -----------------------------------------------


            #region -------MAKE GRID---------

            //----------------------GRID LIST---------------------------
            List<NewGridx> gridX = new List<NewGridx>();
            gridX.Add(new NewGridx("1", 0));
            int m = 1;
            foreach (var item in listG)
            {
                m += 1;
                NewGridx newGridX = new NewGridx();
                newGridX.gNameX = Convert.ToString(m);
                newGridX.gDistanceX = item.wideLen;
                gridX.Add(newGridX);

            }

            List<NewGridx> gridX_mirror = new List<NewGridx>();
            gridX_mirror.Add(new NewGridx(Convert.ToString(spanInt+1), 0));
            int n = spanInt + 1;
            int k = spanInt;
            foreach (var item in listG)
            {
                n -= 1;
                k -= 1;
                NewGridx newGridX = new NewGridx();
                newGridX.gNameX = Convert.ToString(n);
                newGridX.gDistanceX = listG[k].wideLen;
                gridX_mirror.Add(newGridX);
            }

            List<NewGridy> gridY = new List<NewGridy>();
            gridY.Add(new NewGridy("A", 0));
            gridY.Add(new NewGridy("B", listG[0].depthLen1));
            gridY.Add(new NewGridy("C", listG[0].depthLen2));

            List<NewGridy> gridY_mirror = new List<NewGridy>();
            gridY_mirror.Add(new NewGridy("C", 0));
            gridY_mirror.Add(new NewGridy("B", listG[0].depthLen2));
            gridY_mirror.Add(new NewGridy("A", listG[0].depthLen1));

            //----------------------OUTPUT GRID---------------------------

            double exLine = 1840;
            double mPointX_L = -(listG[0].depthLen1 + listG[0].depthLen2 + 7000);
            double mPointX_R = 0;
            foreach (var item in gridX)
            {
                mPointX_R += item.gDistanceX;
            }

            // ARC _ 側 面 立 面 図 _ LEFT ELEVATION
            cmdgridABC(gridY_mirror, listG, exLine, mPointX_L, 22000);        // (-7000, 22000, 0)

            // ARC _ 側 面 立 面 図 _ RIGHT ELEVATION
            cmdgridABC(gridY, listG, exLine, mPointX_R + 7000, 22000);        // (0, 22000, 7000)

            // ARC _ 2 側 面 立 面 図 _ SECTION GRID
            // VKC + VGC + VGC
            // VGC + VGC + VKC
            // VGC + VGC + VKC
            for (int i = 0; i < spanInt; i++)
            {
                if (i == 0 && listG[0].ygName.Contains("VKC"))
                {
                    // LEFT ELEVATION
                    cmdgridABC(gridY_mirror, listG, exLine, mPointX_L, 14000);     // (-7000, 14000, 0)
                }
                else if (i == spanInt - 1 && listG[spanInt - 1].ygName.Contains("VKC"))
                {
                    // RIGHT ELEVATION
                    cmdgridABC(gridY, listG, exLine, mPointX_R + 7000, 14000);    // (0, 14000, 7000)
                }
                else if (i != 0 && i != spanInt - 1 && listG[i].ygName.Contains("VKC"))
                {
                    // LEFT ELEVATION
                    cmdgridABC(gridY_mirror, listG, exLine, mPointX_L, 14000);     // (-7000, 14000, 0)
                    // RIGHT ELEVATION
                    cmdgridABC(gridY, listG, exLine, mPointX_R + 7000, 14000);    // (0, 14000, 7000)
                }
            }

            // ARC _ 正 面 立 面 図 _ FONT ELEVATION
            cmdgrid123(gridX, listG, exLine, 22000);                          // (0, 22000, 0)

            // ARC _ 正 面 立 面 図 _ BACK ELEVATION
            cmdgrid123(gridX_mirror, listG, exLine, 14000);                   // (0, 14000, 0)

            // ARC _ 平 面 図 _ PLAN
            cmdgridABC123(gridX, gridY, listG, 1200, 1200, 0);                // (0, 0, 0)

            // ARC _ 小 屋 伏 図 _ ROOF
            cmdgridABC123(gridX, gridY, listG, 1200, 1200, -14000);          // (0, -14000, 0)

            // STR _ 布 基 礎 伏 図  _ FOUNDATION
            cmdgridABC123(gridX, gridY, listG, 1500, 1500, -28000);          // (0, -22000, 0)

            // STR _ 正　面　断　面　図 _ SECTION
            cmdgrid123(gridX, listG, exLine, -36000);                        // (0, -36000, 0)

            // STR _ 側　面　断　面　図 _ SECTION
            cmdgridABC(gridY_mirror, listG, exLine, mPointX_L, -36000);      // (-7000, -36000, 0)

            #endregion




            // Put your command code here
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed;
            if (doc != null)
            {
                ed = doc.Editor;
                ed.WriteMessage("Hello, this is your first command.");

            }
        }

        public void cmdgridABC123(List<NewGridx> newGridsX, List<NewGridy> newGridsY, List<SettingData> listG, double exLineX, double exLineY, double mPoint)
        {
            var total = newGridsX.Count;
            var totalY = newGridsY.Count;

            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //Write code below here
                ObjectId blkId = db.CurrentSpaceId;
                BlockTableRecord curSpaceBlk = trans.GetObject(blkId, OpenMode.ForWrite) as BlockTableRecord;
                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (!acBlkTbl.Has("grName"))
                {
                    using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                    {
                        acBlkTblRec.Name = "grName";

                        // Set the insertion point for the block
                        acBlkTblRec.Origin = new Point3d(0, 0, 0);

                        // Add a circle to the block
                        using (Circle acCirc = new Circle())
                        {
                            acCirc.Center = new Point3d(0, 0, 0);
                            acCirc.Radius = 200;
                            //acCirc.Layer = "1-8";
                            acCirc.Color = Color.FromColorIndex(ColorMethod.ByColor, 4);
                            acCirc.LinetypeId = db.ContinuousLinetype;

                            acBlkTblRec.AppendEntity(acCirc);

                            // Add an attribute definition to the block
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = new Point3d(0, 0, 0);
                                acAttDef.Prompt = "Grid #: ";
                                acAttDef.Tag = "Grid#";
                                acAttDef.TextString = "grName";
                                acAttDef.Height = 160;
                                //acAttDef.Layer = "2-0";
                                acAttDef.Color = Color.FromColorIndex(ColorMethod.ByColor, 7);
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlkTblRec.AppendEntity(acAttDef);

                                trans.GetObject(db.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlkTblRec);
                                trans.AddNewlyCreatedDBObject(acBlkTblRec, true);
                            }
                        }

                        blkId = acBlkTblRec.Id;
                    }
                }
                else
                {
                    blkId = acBlkTbl["grName"];
                }


                #region //**X Grid**
                List<double> gCordinatesX = new List<double>();
                double gCordinateX = 0.0;
                foreach (var makeGridX in newGridsX)
                {
                    gCordinateX += makeGridX.gDistanceX;
                    gCordinatesX.Add(gCordinateX);

                    // **X -create lines**
                    
                    double yLen = listG[0].depthLen1 + listG[0].depthLen1 + exLineY;
                    Point3d pnt1 = new Point3d(gCordinateX, 0 + mPoint, 0);
                    Point3d pnt2 = new Point3d(gCordinateX, yLen+ mPoint, 0);
                    Line lineObj = new Line(pnt1, pnt2);
                    //lineObj.Layer = "2-0";
                    lineObj.Color = Color.FromColorIndex(ColorMethod.ByColor, 6);
                    //lineObj.LineWeight = LineWeight.LineWeight000;
                    //lineObj.Linetype = "CENTER2";

                    curSpaceBlk.AppendEntity(lineObj);
                    trans.AddNewlyCreatedDBObject(lineObj, true);

                    // Create and insert the new block reference
                    if (blkId != ObjectId.Null)
                    {
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = trans.GetObject(blkId, OpenMode.ForRead) as BlockTableRecord;

                        using (BlockReference acBlkRef = new BlockReference(new Point3d(gCordinateX, yLen+ 200 + mPoint, 0), acBlkTblRec.Id))
                        {
                            BlockTableRecord acCurSpaceBlkTblRec;
                            acCurSpaceBlkTblRec = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                            acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                            trans.AddNewlyCreatedDBObject(acBlkRef, true);

                            // Verify block table record has attribute definitions associated with it
                            if (acBlkTblRec.HasAttributeDefinitions)
                            {
                                // Add attributes from the block table record
                                foreach (ObjectId objID in acBlkTblRec)
                                {
                                    DBObject dbObj = trans.GetObject(objID, OpenMode.ForRead) as DBObject;

                                    if (dbObj is AttributeDefinition)
                                    {
                                        AttributeDefinition acAtt = dbObj as AttributeDefinition;

                                        if (!acAtt.Constant)
                                        {
                                            using (AttributeReference acAttRef = new AttributeReference())
                                            {
                                                acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                                                acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);

                                                acAttRef.TextString = makeGridX.gNameX;

                                                acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                                                trans.AddNewlyCreatedDBObject(acAttRef, true);
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }


                #endregion

                try
                {
                    #region //**Y Grid**
                    List<double> gCordinatesY = new List<double>();
                    double gCordinateY = 0.0;
                    foreach (var makeGridY in newGridsY)
                    {
                        gCordinateY += makeGridY.gDistanceY;
                        gCordinatesY.Add(gCordinateY);

                        // **Y -create lines**
                        Point3d pnt1 = new Point3d(-exLineX, gCordinateY + mPoint, 0);
                        Point3d pnt2 = new Point3d(gCordinatesX[total - 1], gCordinateY + mPoint, 0);
                        Line lineObj = new Line(pnt1, pnt2);
                        //lineObj.Layer = "2-0";
                        lineObj.Color = Color.FromColorIndex(ColorMethod.ByColor, 6);
                        //lineObj.LineWeight = LineWeight.LineWeight000;
                        //lineObj.Linetype = "CENTER2";

                        curSpaceBlk.AppendEntity(lineObj);
                        trans.AddNewlyCreatedDBObject(lineObj, true);

                        // Create and insert the new block reference
                        if (blkId != ObjectId.Null)
                        {
                            BlockTableRecord acBlkTblRec;
                            acBlkTblRec = trans.GetObject(blkId, OpenMode.ForRead) as BlockTableRecord;

                            using (BlockReference acBlkRef = new BlockReference(new Point3d(-exLineX-200, gCordinateY + mPoint, 0), acBlkTblRec.Id))
                            {
                                BlockTableRecord acCurSpaceBlkTblRec;
                                acCurSpaceBlkTblRec = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                                acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                                trans.AddNewlyCreatedDBObject(acBlkRef, true);

                                // Verify block table record has attribute definitions associated with it
                                if (acBlkTblRec.HasAttributeDefinitions)
                                {
                                    // Add attributes from the block table record
                                    foreach (ObjectId objID in acBlkTblRec)
                                    {
                                        DBObject dbObj = trans.GetObject(objID, OpenMode.ForRead) as DBObject;

                                        if (dbObj is AttributeDefinition)
                                        {
                                            AttributeDefinition acAtt = dbObj as AttributeDefinition;

                                            if (!acAtt.Constant)
                                            {
                                                using (AttributeReference acAttRef = new AttributeReference())
                                                {
                                                    acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                                                    acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);

                                                    acAttRef.TextString = makeGridY.gNameY;

                                                    acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                                                    trans.AddNewlyCreatedDBObject(acAttRef, true);
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                    #endregion
                }
                catch
                {
                }
                trans.Commit();
            }

        }

        public void cmdgrid123(List<NewGridx> newGridsX, List<SettingData> listG, double exLine, double mPoint)
        {
            var total = newGridsX.Count;

            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //Write code below here
                ObjectId blkId = db.CurrentSpaceId;
                BlockTableRecord curSpaceBlk = trans.GetObject(blkId, OpenMode.ForWrite) as BlockTableRecord;
                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (!acBlkTbl.Has("grName"))
                {
                    using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                    {
                        acBlkTblRec.Name = "grName";

                        // Set the insertion point for the block
                        acBlkTblRec.Origin = new Point3d(0, 0, 0);

                        // Add a circle to the block
                        using (Circle acCirc = new Circle())
                        {
                            acCirc.Center = new Point3d(0, 0, 0);
                            acCirc.Radius = 200;
                            //acCirc.Layer = "1-8";
                            acCirc.Color = Color.FromColorIndex(ColorMethod.ByColor, 4);
                            acCirc.LinetypeId = db.ContinuousLinetype;

                            acBlkTblRec.AppendEntity(acCirc);

                            // Add an attribute definition to the block
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = new Point3d(0, 0, 0);
                                acAttDef.Prompt = "Grid #: ";
                                acAttDef.Tag = "Grid#";
                                acAttDef.TextString = "grName";
                                acAttDef.Height = 160;
                                //acAttDef.Layer = "2-0";
                                acAttDef.Color = Color.FromColorIndex(ColorMethod.ByColor, 7);
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlkTblRec.AppendEntity(acAttDef);

                                trans.GetObject(db.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlkTblRec);
                                trans.AddNewlyCreatedDBObject(acBlkTblRec, true);
                            }
                        }

                        blkId = acBlkTblRec.Id;
                    }
                }
                else
                {
                    blkId = acBlkTbl["grName"];
                }


                #region //**X Grid**
                List<double> gCordinatesX = new List<double>();
                double gCordinateX = 0.0;
                foreach (var makeGridX in newGridsX)
                {
                    gCordinateX += makeGridX.gDistanceX;
                    gCordinatesX.Add(gCordinateX);

                    // **X -create lines**

                    double yLen = listG[0].depthLen1 + listG[0].depthLen1 + exLine;
                    Point3d pnt1 = new Point3d(gCordinateX, -exLine + mPoint, 0);
                    Point3d pnt2 = new Point3d(gCordinateX, 1000 + mPoint, 0);
                    Line lineObj = new Line(pnt1, pnt2);
                    //lineObj.Layer = "2-0";
                    lineObj.Color = Color.FromColorIndex(ColorMethod.ByColor, 6);
                    //lineObj.LineWeight = LineWeight.LineWeight000;
                    //lineObj.Linetype = "CENTER2";

                    curSpaceBlk.AppendEntity(lineObj);
                    trans.AddNewlyCreatedDBObject(lineObj, true);

                    // Create and insert the new block reference
                    if (blkId != ObjectId.Null)
                    {
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = trans.GetObject(blkId, OpenMode.ForRead) as BlockTableRecord;

                        using (BlockReference acBlkRef = new BlockReference(new Point3d(gCordinateX, -exLine - 200 + mPoint, 0), acBlkTblRec.Id))
                        {
                            BlockTableRecord acCurSpaceBlkTblRec;
                            acCurSpaceBlkTblRec = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                            acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                            trans.AddNewlyCreatedDBObject(acBlkRef, true);

                            // Verify block table record has attribute definitions associated with it
                            if (acBlkTblRec.HasAttributeDefinitions)
                            {
                                // Add attributes from the block table record
                                foreach (ObjectId objID in acBlkTblRec)
                                {
                                    DBObject dbObj = trans.GetObject(objID, OpenMode.ForRead) as DBObject;

                                    if (dbObj is AttributeDefinition)
                                    {
                                        AttributeDefinition acAtt = dbObj as AttributeDefinition;

                                        if (!acAtt.Constant)
                                        {
                                            using (AttributeReference acAttRef = new AttributeReference())
                                            {
                                                acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                                                acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);

                                                acAttRef.TextString = makeGridX.gNameX;

                                                acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                                                trans.AddNewlyCreatedDBObject(acAttRef, true);
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }


                #endregion

                trans.Commit();
            }

        }

        public void cmdgridABC(List<NewGridy> newGridsY, List<SettingData> listG, double exLine, double mPointX, double mPointY)
        {
            var total = newGridsY.Count;

            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //Write code below here
                ObjectId blkId = db.CurrentSpaceId;
                BlockTableRecord curSpaceBlk = trans.GetObject(blkId, OpenMode.ForWrite) as BlockTableRecord;
                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (!acBlkTbl.Has("grName"))
                {
                    using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                    {
                        acBlkTblRec.Name = "grName";

                        // Set the insertion point for the block
                        acBlkTblRec.Origin = new Point3d(0, 0, 0);

                        // Add a circle to the block
                        using (Circle acCirc = new Circle())
                        {
                            acCirc.Center = new Point3d(0, 0, 0);
                            acCirc.Radius = 200;
                            //acCirc.Layer = "1-8";
                            acCirc.Color = Color.FromColorIndex(ColorMethod.ByColor, 4);
                            acCirc.LinetypeId = db.ContinuousLinetype;

                            acBlkTblRec.AppendEntity(acCirc);

                            // Add an attribute definition to the block
                            using (AttributeDefinition acAttDef = new AttributeDefinition())
                            {
                                acAttDef.Position = new Point3d(0, 0, 0);
                                acAttDef.Prompt = "Grid #: ";
                                acAttDef.Tag = "Grid#";
                                acAttDef.TextString = "grName";
                                acAttDef.Height = 160;
                                //acAttDef.Layer = "2-0";
                                acAttDef.Color = Color.FromColorIndex(ColorMethod.ByColor, 7);
                                acAttDef.Justify = AttachmentPoint.MiddleCenter;
                                acBlkTblRec.AppendEntity(acAttDef);

                                trans.GetObject(db.BlockTableId, OpenMode.ForWrite);
                                acBlkTbl.Add(acBlkTblRec);
                                trans.AddNewlyCreatedDBObject(acBlkTblRec, true);
                            }
                        }

                        blkId = acBlkTblRec.Id;
                    }
                }
                else
                {
                    blkId = acBlkTbl["grName"];
                }


                #region //**X Grid**
                List<double> gCordinatesX = new List<double>();
                double gCordinateX = 0.0;
                foreach (var makeGridX in newGridsY)
                {
                    gCordinateX += makeGridX.gDistanceY;
                    gCordinatesX.Add(gCordinateX);

                    // **X -create lines**

                    double yLen = listG[0].depthLen1 + listG[0].depthLen1 + exLine;
                    Point3d pnt1 = new Point3d(gCordinateX + mPointX, -exLine + mPointY, 0);
                    Point3d pnt2 = new Point3d(gCordinateX + mPointX, 1000 + mPointY, 0);
                    Line lineObj = new Line(pnt1, pnt2);
                    //lineObj.Layer = "2-0";
                    lineObj.Color = Color.FromColorIndex(ColorMethod.ByColor, 6);
                    //lineObj.LineWeight = LineWeight.LineWeight000;
                    //lineObj.Linetype = "CENTER2";

                    curSpaceBlk.AppendEntity(lineObj);
                    trans.AddNewlyCreatedDBObject(lineObj, true);

                    // Create and insert the new block reference
                    if (blkId != ObjectId.Null)
                    {
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = trans.GetObject(blkId, OpenMode.ForRead) as BlockTableRecord;

                        using (BlockReference acBlkRef = new BlockReference(new Point3d(gCordinateX + mPointX, -exLine - 200 + mPointY, 0), acBlkTblRec.Id))
                        {
                            BlockTableRecord acCurSpaceBlkTblRec;
                            acCurSpaceBlkTblRec = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                            acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                            trans.AddNewlyCreatedDBObject(acBlkRef, true);

                            // Verify block table record has attribute definitions associated with it
                            if (acBlkTblRec.HasAttributeDefinitions)
                            {
                                // Add attributes from the block table record
                                foreach (ObjectId objID in acBlkTblRec)
                                {
                                    DBObject dbObj = trans.GetObject(objID, OpenMode.ForRead) as DBObject;

                                    if (dbObj is AttributeDefinition)
                                    {
                                        AttributeDefinition acAtt = dbObj as AttributeDefinition;

                                        if (!acAtt.Constant)
                                        {
                                            using (AttributeReference acAttRef = new AttributeReference())
                                            {
                                                acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                                                acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);

                                                acAttRef.TextString = makeGridX.gNameY;

                                                acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                                                trans.AddNewlyCreatedDBObject(acAttRef, true);
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }


                #endregion

                trans.Commit();
            }

        }

        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "MyPickFirst", "MyPickFirstLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MyPickFirst() // This method can have any name
        {
            PromptSelectionResult result = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection();
            if (result.Status == PromptStatus.OK)
            {
                // There are selected entities
                // Put your command using pickfirst set code here
            }
            else
            {
                // There are no selected entities
                // Put your command code here
            }
        }

        // Application Session Command with localized name
        [CommandMethod("MyGroup", "MySessionCmd", "MySessionCmdLocal", CommandFlags.Modal | CommandFlags.Session)]
        public void MySessionCmd() // This method can have any name
        {
            // Put your command code here
        }

        // LispFunction is similar to CommandMethod but it creates a lisp 
        // callable function. Many return types are supported not just string
        // or integer.
        [LispFunction("MyLispFunction", "MyLispFunctionLocal")]
        public int MyLispFunction(ResultBuffer args) // This method can have any name
        {
            // Put your command code here

            // Return a value to the AutoCAD Lisp Interpreter
            return 1;
        }


    }

}
