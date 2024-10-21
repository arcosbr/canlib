unit main;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ComCtrls, ImgList, ToolWin, ExtCtrls, System.ImageList;

type
  TMainForm = class(TForm)
    ListView: TListView;
    ToolBar1: TToolBar;
    RefreshBtn: TToolButton;
    ImageList1: TImageList;
    Panel1: TPanel;
    procedure FormShow(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure RefreshBtnClick(Sender: TObject);
    procedure ListViewDblClick(Sender: TObject);
    procedure ListViewKeyPress(Sender: TObject; var Key: Char);
    procedure FormKeyPress(Sender: TObject; var Key: Char);
  private
    { Private declarations }
  public
    procedure PopulateList;
    function  HwTypeToString(hw_Type: Integer): string;
    procedure DisplayDetails;
    procedure WM__DeviceChange(var msg: TMessage); message WM_DEVICECHANGE;
  end;

var
  MainForm: TMainForm;

implementation

{$R *.DFM}
{$R ICON.RES}

uses CANLIB, detail;

procedure TMainForm.WM__DeviceChange(var msg: TMessage);
begin
  PopulateList;
  msg.Result := 1;
end;

function TMainForm.HwTypeToString(hw_Type: Integer): string;
var
  i: Integer;
  stat: canStatus;
  hwName: AnsiString;
  hwType: Integer;
  hwBusType: Integer;
  outbuf: string;

begin
  i := 0;
  SetLength(hwName, 50);
  outbuf := 'Unknown??';
  repeat
    stat := kvGetSupportedInterfaceInfo(i, @hwName[1], 50, &hwType, &hwBusType);
    if ((stat = canOK) and (hw_Type = hwType)) then begin
      break;
    end;
    inc(i);
  until (not (stat = canOK));
  outbuf := String(hwName);
  Result := outbuf;
end;

procedure TMainForm.PopulateList;
var i: Integer;
  numChannels: Integer;
  item: TListItem;
  s: string;
  p: AnsiString;
  n: Integer;
  revno: array[0..3] of word;
begin
  canUnloadLibrary;
  canInitializeLibrary;
  SetLength(p, 64);

  canGetNumberOfChannels(numChannels);
  ListView.Items.Clear;
  try
    Screen.Cursor := crHourGlass;
    ListView.Items.BeginUpdate;
    for i := 0 to numChannels - 1 do begin
      item := ListView.Items.Add;

      item.Caption := Format('%d', [i]);

      canGetChannelData(i, canCHANNELDATA_CHAN_NO_ON_CARD, n, sizeof(n));
      item.SubItems.Add(IntToStr(n));

      canGetChannelData(i, canCHANNELDATA_DEVDESCR_ASCII, p[1], 64);
      item.SubItems.Add(string(p));

      canGetChannelData(i, canCHANNELDATA_CHANNEL_NAME, p[1], 64);
      item.SubItems.Add(string(p));

      canGetChannelData(i, canCHANNELDATA_CARD_TYPE, n, sizeof(n));

      s := HwTypeToString(n);

      item.SubItems.Add(s);

      canGetChannelData(i, canCHANNELDATA_CARD_SERIAL_NO, n, sizeof(n));
      item.SubItems.Add(IntToStr(n));

      canGetChannelData(i, canCHANNELDATA_CARD_FIRMWARE_REV, revno, sizeof(revno));
      item.SubItems.Add(Format('%d.%d.%d', [revno[3], revno[2], revno[0]]));
    end;
  finally
    Screen.Cursor := crDefault;
  end;
  ListView.Items.EndUpdate;
end;

procedure TMainForm.FormShow(Sender: TObject);
begin
  PopulateList;
end;

procedure TMainForm.FormCreate(Sender: TObject);
begin
  canInitializeLibrary;
end;

procedure TMainForm.RefreshBtnClick(Sender: TObject);
begin
  PopulateList;
end;

procedure TMainForm.ListViewDblClick(Sender: TObject);
begin
  DisplayDetails;
end;

procedure TMainForm.ListViewKeyPress(Sender: TObject; var Key: Char);
begin
  if Key = chr(13) then DisplayDetails;
end;

procedure TMainForm.DisplayDetails;
begin
  if ListView.SelCount <> 1 then Exit;
  DetailForm.channel := ListView.Selected.Index;
  DetailForm.ShowModal;
end;

procedure TMainForm.FormKeyPress(Sender: TObject; var Key: Char);
begin
  if Key = chr(27) then Close;
end;

end.
