namespace Bento4Sharp;

public enum Bento4Result
{
    Success = 0,
    Failure = -1,
    OutOfMemory = -2,
    InvalidParameters = -3,
    NoSuchFile = -4,
    PermissionDenied = -5,
    CannotOpenFile = -6,
    EndOfStream = -7,
    WriteFailed = -8,
    ReadFailed = -9,
    InvalidFormat = -10,
    NoSuchItem = -11,
    OutOfRange = -12,
    Internal = -13,
    InvalidState = -14,
    ListEmpty = -15,
    ListOperationAborted = -16,
    InvalidRtpConstructorType = -17,
    NotSupported = -18,
    InvalidTrackType = -19,
    InvalidRtpPacketExtraData = -20,
    BufferTooSmall = -21,
    NotEnoughData = -22
}
