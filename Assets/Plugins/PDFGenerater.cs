using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class PDFGenerater : MonoBehaviour {

	iTextSharp.text.Document pdfdoc;  
	iTextSharp.text.Image pdfImg;  
	iTextSharp.text.pdf.PdfWriter pdfwriter; 

	iTextSharp.text.Rectangle currentPageSize = new iTextSharp.text.Rectangle (7440,12360);//iTextSharp.text.PageSize.A0;


	int gridSideLength = 20;// side length of each small square grid
	int blockRowInterval = 40;//interval between two BlockRow
	int gridCountInBlockRowVertical = 40;
	int gridCountInBlockRowHorizontal;// = currentPageSize.Width / gridSideLength;
	int firstBlockRowOffsetInVertical = 100;//first BlockRow move down to give space for Text Head writing
	

	int blockRowHeight; // = gridSideLength * gridCountInBlockRowVertical;
	int blockRowCount; // = currentPageSize.Height / (blockRowHeight + blockRowInterval);   means how many BlockRow each page contains

	
	int blockRowIndex = 0;
	float currentBaseLine;

	// means how many raw data will be contained within a BlockRow
	int rawDataCountInBlockRow = 0;// = gridCountInBlockRowHorizontal * 40ms * 512 / 1000ms;
	//a small square == 40ms in X-axle, 1 second == 512 raw samples                     
	float rawDataInterval = 0;// = currentPageSize.Width / rawDataCountInBlockRow;
	int rawDataIndex = 0; // if rawDataIndex >= rawDataCountInBlockRow; blockRowIndex++ to change to next BlockRow


	int maxRawData = 13990;//a small square == 0.1mv in Y-axle, 0.1mv == 699.5; there are 20 small square above/under baseLine.
	float scaledRawData = 0;// = (rawData * (blockRowHeight / 2)) / maxRawData;  zoom RawData to small, less that BlockRowHeight / 2;  



	// Use this for initialization
	void Start () {


		//CreateFromFolder (Application.persistentDataPath);
		CreateFromRawdataFile ("/Volumes/Work/RawData.txt","/Volumes/Work/outpdf.pdf");
	}

	void CreateFromRawdataFile(string sourceTxtPath, string destinationPDFPath){
		pdfdoc = new iTextSharp.text.Document ();
		pdfdoc.SetPageSize (currentPageSize);

		gridCountInBlockRowHorizontal = (int)(currentPageSize.Width / gridSideLength);
		blockRowHeight = gridSideLength * gridCountInBlockRowVertical;
		blockRowCount = (int)(currentPageSize.Height / (blockRowHeight + blockRowInterval));
		rawDataCountInBlockRow = (int)(gridCountInBlockRowHorizontal * 40 * 512 / 1000);
		rawDataInterval = currentPageSize.Width / rawDataCountInBlockRow;

		Debug.Log ("gridCountInBlockRowHorizontal:" + gridCountInBlockRowHorizontal);
		Debug.Log ("rawDataCountInBlockRow" + rawDataCountInBlockRow);

		if (File.Exists (destinationPDFPath)) {
			File.Delete(destinationPDFPath);
		}
		
		pdfwriter = iTextSharp.text.pdf.PdfWriter.GetInstance (pdfdoc,new FileStream(destinationPDFPath,FileMode.CreateNew));

		pdfdoc.Open ();

		iTextSharp.text.pdf.PdfContentByte cb = pdfwriter.DirectContent;

		pdfdoc.NewPage ();
		blockRowIndex = 0;
		rawDataIndex = 0;

		iTextSharp.text.pdf.BaseFont font = iTextSharp.text.pdf.BaseFont.CreateFont();
		cb.SetFontAndSize (font,60);
		cb.BeginText ();
		cb.SetTextMatrix (100,currentPageSize.Height - 50);
		cb.ShowText ("Test String .......");
		cb.EndText ();

		System.IO.StreamReader reader = new StreamReader (sourceTxtPath);
		string theLine;
		int rawData;

		while(!reader.EndOfStream){
			theLine = reader.ReadLine();
			try{
				rawData = int.Parse(theLine);
			}catch(Exception e){
				Debug.Log("Exception");
				continue;

			}

			rawData = rawData > maxRawData ? maxRawData : rawData;
			rawData = rawData < -maxRawData ? -maxRawData : rawData;
			scaledRawData = (rawData * (blockRowHeight / 2)) / maxRawData;

			if(rawDataIndex == 0){
				currentBaseLine = currentPageSize.Height - (firstBlockRowOffsetInVertical + blockRowIndex * (blockRowHeight + blockRowInterval) + (blockRowHeight / 2));
				
				cb.SetColorStroke (iTextSharp.text.BaseColor.RED);
				
				//draw horizontal lines
				for(int i = 0; i <= gridCountInBlockRowVertical; i++){
					if(i % 5 == 0){
						cb.SetLineWidth (2.5f);
					}else{
						cb.SetLineWidth (0.5f);
					}
					cb.MoveTo(0,currentPageSize.Height - (firstBlockRowOffsetInVertical + blockRowIndex * (blockRowHeight + blockRowInterval) + i * gridSideLength));
					cb.LineTo(currentPageSize.Width,currentPageSize.Height - (firstBlockRowOffsetInVertical + blockRowIndex * (blockRowHeight + blockRowInterval) + i * gridSideLength));
					cb.Stroke();
				}
				
				//draw vertical lines
				for(int j = 0; j <= gridCountInBlockRowHorizontal; j++){
					if(j % 5 == 0){
						cb.SetLineWidth (2.5f);
					}else{
						cb.SetLineWidth (0.5f);
					}
					cb.MoveTo(j * gridSideLength,currentPageSize.Height - (firstBlockRowOffsetInVertical + blockRowIndex * (blockRowHeight + blockRowInterval)));
					cb.LineTo(j * gridSideLength,currentPageSize.Height - (firstBlockRowOffsetInVertical + blockRowIndex * (blockRowHeight + blockRowInterval) + blockRowHeight));
					cb.Stroke();
				}
				//prepare to draw ECG 
				cb.SetLineWidth (1.5f);
				cb.SetColorStroke (iTextSharp.text.BaseColor.BLACK);
				
				cb.MoveTo (0,currentBaseLine);
			}

			cb.LineTo (rawDataIndex * rawDataInterval,currentBaseLine + scaledRawData);
			rawDataIndex++;
			if(rawDataIndex >= rawDataCountInBlockRow){
				cb.Stroke();
				rawDataIndex = 0;
				blockRowIndex++;
			}

		}
		cb.Stroke();
		reader.Close ();








		pdfdoc.Dispose();
		System.Diagnostics.Process.Start (destinationPDFPath);
	}

	void CreateFromFolder(string folderPath){
		pdfdoc = new iTextSharp.text.Document ();
		pdfdoc.SetPageSize (iTextSharp.text.PageSize.A0);

		Debug.Log ("000000");

		string pdfPath = Path.Combine (folderPath,"output.pdf");
		if (File.Exists (pdfPath)) {
		
			File.Delete(pdfPath);
		}
		Debug.Log ("1111111");

		pdfwriter = iTextSharp.text.pdf.PdfWriter.GetInstance (pdfdoc,new FileStream(pdfPath,FileMode.CreateNew));

		Debug.Log ("22222");
		pdfdoc.Open ();

		pdfdoc.NewPage ();

		string[] paths = Directory.GetFiles (folderPath);

		int rows;
		int columns;

		int rowIndex = 1;
		int pageIndex = 1;

		float pageWidth = iTextSharp.text.PageSize.A0.Width;
		float pageHeight = iTextSharp.text.PageSize.A0.Height;

		pdfImg = iTextSharp.text.Image.GetInstance(paths[0]);
		
		rows = (int)(pageHeight/pdfImg.Height);
		columns = (int)(pageWidth/pdfImg.Width);
		Debug.Log ("Rows:" + rows);
		Debug.Log ("Columns:" + columns);

		for(int i = 0; i < paths.Length; i++){
			if(paths[i].EndsWith(".png")){
				Debug.Log(paths[i]);
				pdfImg = iTextSharp.text.Image.GetInstance(paths[i]);

				if(i == columns * rows * pageIndex){
					pageIndex++;
					Debug.Log("pageIndex:" + pageIndex);

					pdfdoc.NewPage();
					rowIndex = 1;
					Debug.Log("rowIndex:" + rowIndex);
				}else if(i != 0 && i % columns == 0){
					rowIndex++;
					Debug.Log("rowIndex:" + rowIndex);
				}
				pdfImg.SetAbsolutePosition(pdfImg.Width * (i % columns),pageHeight - pdfImg.Height * rowIndex);

				pdfdoc.Add(pdfImg);
			}
		}

		pdfdoc.Dispose(); 
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
