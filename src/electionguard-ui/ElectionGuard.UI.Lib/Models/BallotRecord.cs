using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Lib.Models;

public partial class BallotRecord : DatabaseRecord
{
    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _uploadId;

    [ObservableProperty]
    private string? _fileName;

    [ObservableProperty]
    private string? _ballotData;

    public BallotRecord(string electionId, string uploadId, string fileName, string ballotData) : base(nameof(BallotRecord))
    {
        ElectionId = electionId;
        UploadId = uploadId;
        FileName = fileName;
        BallotData = ballotData;
    }
    public BallotRecord() : base(nameof(BallotRecord))
    {
    }

}
