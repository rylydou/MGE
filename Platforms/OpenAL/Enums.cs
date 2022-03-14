namespace MGE.OpenAL;

public enum ALFormat : int
{
	Mono8 = 0x1100,
	Mono16 = 0x1101,
	Stereo8 = 0x1102,
	Stereo16 = 0x1103,
	MonoIma4 = 0x1300,
	StereoIma4 = 0x1301,
	MonoMSAdpcm = 0x1302,
	StereoMSAdpcm = 0x1303,
	MonoFloat32 = 0x10010,
	StereoFloat32 = 0x10011,
}

public enum ALError : int
{
	NoError = 0,
	InvalidName = 0xA001,
	InvalidEnum = 0xA002,
	InvalidValue = 0xA003,
	InvalidOperation = 0xA004,
	OutOfMemory = 0xA005,
}

public enum ALGetString : int
{
	Extensions = 0xB004,
}

public enum ALBufferi : int
{
	UnpackBlockAlignmentSoft = 0x200C,
	LoopSoftPointsExt = 0x2015,
}

public enum ALGetBufferi : int
{
	Bits = 0x2002,
	Channels = 0x2003,
	Size = 0x2004,
}

public enum ALSourceb : int
{
	Looping = 0x1007,
}

public enum ALSourcei : int
{
	SourceRelative = 0x202,
	Buffer = 0x1009,
	EfxDirectFilter = 0x20005,
	EfxAuxilarySendFilter = 0x20006,
}

public enum ALSourcef : int
{
	Pitch = 0x1003,
	Gain = 0x100A,
	ReferenceDistance = 0x1020,
	MaxDistance = 0x1023,
}

public enum ALGetSourcei : int
{
	SampleOffset = 0x1025,
	SourceState = 0x1010,
	BuffersQueued = 0x1015,
	BuffersProcessed = 0x1016,
}

public enum ALSourceState : int
{
	Initial = 0x1011,
	Playing = 0x1012,
	Paused = 0x1013,
	Stopped = 0x1014,
}

public enum ALListener3f : int
{
	Position = 0x1004,
	Velocity = 0x1006,
}

public enum ALSource3f : int
{
	Position = 0x1004,
	Velocity = 0x1006,
}

public enum ALDistanceModel : int
{
	None = 0,
	InverseDistanceClamped = 0xD002,
}

public enum ALCError : int
{
	NoError = 0,
}

public enum ALCGetString : int
{
	CaptureDeviceSpecifier = 0x0310,
	CaptureDefaultDeviceSpecifier = 0x0311,
	Extensions = 0x1006,
}

public enum ALCGetInteger : int
{
	CaptureSamples = 0x0312,
}

public enum EfxFilteri : int
{
	FilterType = 0x8001,
}

public enum EfxFilterf : int
{
	LowpassGain = 0x0001,
	LowpassGainHF = 0x0002,
	HighpassGain = 0x0001,
	HighpassGainLF = 0x0002,
	BandpassGain = 0x0001,
	BandpassGainLF = 0x0002,
	BandpassGainHF = 0x0003,
}

public enum EfxFilterType : int
{
	None = 0x0000,
	Lowpass = 0x0001,
	Highpass = 0x0002,
	Bandpass = 0x0003,
}

public enum EfxEffecti : int
{
	EffectType = 0x8001,
	SlotEffect = 0x0001,
}

public enum EfxEffectSlotf : int
{
	EffectSlotGain = 0x0002,
}

public enum EfxEffectf : int
{
	EaxReverbDensity = 0x0001,
	EaxReverbDiffusion = 0x0002,
	EaxReverbGain = 0x0003,
	EaxReverbGainHF = 0x0004,
	EaxReverbGainLF = 0x0005,
	DecayTime = 0x0006,
	DecayHighFrequencyRatio = 0x0007,
	DecayLowFrequencyRation = 0x0008,
	EaxReverbReflectionsGain = 0x0009,
	EaxReverbReflectionsDelay = 0x000A,
	ReflectionsPain = 0x000B,
	LateReverbGain = 0x000C,
	LateReverbDelay = 0x000D,
	LateRevertPain = 0x000E,
	EchoTime = 0x000F,
	EchoDepth = 0x0010,
	ModulationTime = 0x0011,
	ModulationDepth = 0x0012,
	AirAbsorbsionHighFrequency = 0x0013,
	EaxReverbHFReference = 0x0014,
	EaxReverbLFReference = 0x0015,
	RoomRolloffFactor = 0x0016,
	DecayHighFrequencyLimit = 0x0017,
}

public enum EfxEffectType : int
{
	Reverb = 0x8000,
}
