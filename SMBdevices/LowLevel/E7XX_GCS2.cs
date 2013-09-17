using System;
using System.Runtime.InteropServices;
using System.Text;


namespace PI
{

	/// <summary>
	/// Summary description for E7XX_G.
	/// </summary>
	public class GCS2
	{


        ////////////////////////////////
        // E7XX_GCS2 Bits (E7XX_BIT_XXX). //
        ////////////////////////////////

        /* Curve Controll E7XX_BIT_WGO_XXX */
        public const uint E7XX_BIT_WGO_START_DEFAULT			    = 0x00000001;
        public const uint E7XX_BIT_WGO_START_EXTERN_TRIGGER		= 0x00000002;
        public const uint E7XX_BIT_WGO_WITH_DDL_INITIALISATION	= 0x00000040;
        public const uint E7XX_BIT_WGO_WITH_DDL					= 0x00000080;
        public const uint E7XX_BIT_WGO_START_AT_ENDPOSITION		= 0x00000100;
        public const uint E7XX_BIT_WGO_SINGLE_RUN_DDL_TEST		= 0x00000200;
        public const uint E7XX_BIT_WGO_EXTERN_WAVE_GENERATOR	    = 0x00000400;
        public const uint E7XX_BIT_WGO_SAVE_BIT_1				    = 0x00100000;
        public const uint E7XX_BIT_WGO_SAVE_BIT_2				    = 0x00200000;
        public const uint E7XX_BIT_WGO_SAVE_BIT_3				    = 0x00400000;

        /* Wave-Trigger E7XX_BIT_TRG_XXX */
        public const uint E7XX_BIT_TRG_LINE_1					    = 0x0001;
        public const uint E7XX_BIT_TRG_LINE_2					    = 0x0002;
        public const uint E7XX_BIT_TRG_LINE_3					    = 0x0003;
        public const uint E7XX_BIT_TRG_LINE_4					    = 0x0008;
        public const uint E7XX_BIT_TRG_ALL_CURVE_POINTS			= 0x0100;

        /* Data Record Configuration E7XX_DRC_XXX */
        public const uint E7XX_DRC_DEFAULT					    = 0;
        public const uint E7XX_DRC_AXIS_TARGET_POS			    = 1;
        public const uint E7XX_DRC_AXIS_ACTUAL_POS			    = 2;
        public const uint E7XX_DRC_AXIS_POS_ERROR			        = 3;
        public const uint E7XX_DRC_AXIS_DDL_DATA			        = 4;
        public const uint E7XX_DRC_AXIS_DRIVING_VOL			    = 5;
        public const uint E7XX_DRC_PIEZO_MODEL_VOL			    = 6;
        public const uint E7XX_DRC_PIEZO_VOL				        = 7;
        public const uint E7XX_DRC_SENSOR_POS				        = 8;


        /* P(arameter)I(nfo)F(lag)_M(emory)T(ype)_XX */
        public const uint E7XX_PIF_MT_RAM					        = 0x00000001;
        public const uint E7XX_PIF_MT_EPROM					    = 0x00000002;
        public const uint E7XX_PIF_MT_ALL					        = (E7XX_PIF_MT_RAM | E7XX_PIF_MT_EPROM);

        /* P(arameter)I(nfo)F(lag)_D(ata)T(ype)_XX */
        public const uint E7XX_PIF_DT_INT					        = 1;
        public const uint E7XX_PIF_DT_FLOAT					    = 2;
        public const uint E7XX_PIF_DT_CHAR					    = 3;


        /////////////////////////////////////////////////////////////////////////////
        // DLL initialization and comm functions
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_InterfaceSetupDlg")]		    public static extern int	InterfaceSetupDlg(string sRegKeyName);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ConnectRS232")]		        public static extern int	ConnectRS232(int nPortNr, int nBaudRate);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_OpenRS232DaisyChain")]		public static extern int	OpenRS232DaisyChain(int iPortNumber, int iBaudRate, ref int pNumberOfConnectedDaisyChainDevices, StringBuilder sDeviceIDNs, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ConnectDaisyChainDevice")]	public static extern int	ConnectDaisyChainDevice(int iPortId, int iDeviceNumber);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_CloseDaisyChain")]		    public static extern void	CloseDaisyChain(int iPortId);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ConnectNIgpib")]		        public static extern int	ConnectNIgpib(int nBoard, int nDevAddr);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ConnectTCPIP")]		        public static extern int	ConnectTCPIP(string sHostname, int port);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_EnableTCPIPScan")]		    public static extern int	EnableTCPIPScan(int iMask);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_EnumerateTCPIPDevices")]		public static extern int	EnumerateTCPIPDevices(StringBuilder sBuffer, int iBufferSize, string sFilter);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ConnectTCPIPByDescription")]	public static extern int	ConnectTCPIPByDescription(string szDescription);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_EnumerateUSB")]              public static extern int    EnumerateUSB(StringBuilder sBuffer, int iBufferSize, string sFilter);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ConnectUSB")]               public static extern int    ConnectUSB(string sDescription);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ConnectUSBWithBaudRate")]    public static extern int    ConnectUSBWithBaudRate(string sDescription, int iBaudRate);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_OpenUSBDaisyChain")]         public static extern int    OpenUSBDaisyChain(string sDescription, ref int pNumberOfConnectedDaisyChainDevices, StringBuilder sDeviceIDNs, int iBufferSize);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_IsConnected")]		        public static extern int	IsConnected(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_CloseConnection")]		    public static extern int	CloseConnection(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GetError")]		            public static extern int	GetError(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SetErrorCheck")]		        public static extern int	SetErrorCheck(int ID, int bErrorCheck);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_TranslateError")]		    public static extern int	TranslateError(int errNr, StringBuilder sBuffer, int iBufferSize);




        /////////////////////////////////////////////////////////////////////////////
        // general
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qERR")]          public static extern int qERR(int ID, ref int pnError);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qIDN")]		    public static extern int qIDN(int ID, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_INI")]           public static extern int INI(int ID, string sAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qHLP")]          public static extern int qHLP(int ID, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qHPA")]          public static extern int qHPA(int ID, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qHPV")]          public static extern int qHPV(int ID, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qCSV")]          public static extern int qCSV(int ID, ref double dCommandSyntaxVersion);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qOVF")]          public static extern int qOVF(int ID, string sAxes, int[] iValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_RBT")]           public static extern int RBT(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_REP")]           public static extern int REP(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_BDR")]           public static extern int BDR(int ID, int iBaudRate);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qBDR")]          public static extern int qBDR(int ID, ref int iBaudRate);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DBR")]           public static extern int DBR(int ID, int iBaudRate);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDBR")]          public static extern int qDBR(int ID, ref int iBaudRate);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qVER")]          public static extern int qVER(int ID, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSSN")]          public static extern int qSSN(int ID, StringBuilder sSerialNumber, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_CCT")]           public static extern int CCT(int ID, int iCommandType);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qCCT")]          public static extern int qCCT(int ID, ref int iCommandType);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTVI")]          public static extern int qTVI(int ID, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_IFC")]           public static extern int IFC(int ID, string sParameters, string sValues);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qIFC")]          public static extern int qIFC(int ID, string sParameters, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_IFS")]           public static extern int IFS(int ID, string sPassword, string sParameters, string sValues);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qIFS")]          public static extern int qIFS(int ID, string sParameters, StringBuilder sBuffer, int iBufferSize);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MOV")]           public static extern int MOV(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qMOV")]          public static extern int qMOV(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MOV")]           public static extern int MVR(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_POS")]           public static extern int POS(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qPOS")]          public static extern int qPOS(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_IsMoving")]      public static extern int IsMoving(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_HLT")]           public static extern int HLT(int ID, string sAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_STP")]           public static extern int STP(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qONT")]          public static extern int qONT(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_RTO")]           public static extern int RTO(int ID, string sAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qRTO")]          public static extern int qRTO(int ID, string sAxes, int[] iValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ATZ")]           public static extern int ATZ(int ID, string sAxes, double[] dLowvoltageArray, int[] fUseDefaultArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qATZ")]          public static extern int qATZ(int ID, string sAxes, int[] iAtzResultArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_AOS")]          public static extern int AOS(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qAOS")]          public static extern int qAOS(int ID, string sAxes, double[] dValueArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SVA")]           public static extern int SVA(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSVA")]          public static extern int qSVA(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_AVR")]           public static extern int SVR(int ID, string sAxes, double[] dValueArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DFH")]           public static extern int DFH(int ID, string sAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDFH")]          public static extern int qDFH(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GOH")]           public static extern int GOH(int ID, string sAxes);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qCST")]          public static extern int qCST(int ID, string sAxes, StringBuilder sNames, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_CST")]           public static extern int CST(int ID, string sAxes, string sNames);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qVST")]          public static extern int qVST(int ID, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qPUN")]          public static extern int qPUN(int ID, string sAxes, StringBuilder sUnit, int iBufferSize);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SVO")]           public static extern int SVO(int ID, string sAxes, int[] iValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSVO")]          public static extern int qSVO(int ID, string sAxes, int[] iValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SMO")]           public static extern int SMO( int ID, string sAxes, int[] iValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSMO")]          public static extern int qSMO( int ID, string sAxes, int[] iValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DCO")]           public static extern int DCO(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDCO")]          public static extern int qDCO(int ID, string sAxes, int[] bValueArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_RON")]           public static extern int RON(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qRON")]          public static extern int qRON(int ID, string sAxes, int[] bValueArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_VEL")]           public static extern int VEL(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qVEL")]          public static extern int qVEL(int ID, string sAxes, double[] dValueArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ACC")]           public static extern int ACC(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qACC")]          public static extern int qACC(int ID, string sAxes, double[] dValueArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DEC")]           public static extern int DEC(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDEC")]          public static extern int qDEC(int ID, string sAxes, double[] dValueArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_VCO")]           public static extern int VCO(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qVCO")]          public static extern int qVCO(int ID, string sAxes, int[] bValueArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SPA")]           public static extern int SPA(int ID, string sAxes, uint[] iParameterArray, double[] dValueArray, string sStrings);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSPA")]          public static extern int qSPA(int ID, string sAxes, uint[] iParameterArray, double[] dValueArray, StringBuilder sStrings, int iMaxNameSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SEP")]           public static extern int SEP(int ID, string sPassword, string sAxes, uint[] iParameterArray, double[] dValueArray, string sStrings);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSEP")]          public static extern int qSEP(int ID, string sAxes, uint[] iParameterArray, double[] dValueArray, StringBuilder sStrings, int iMaxNameSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WPA")]           public static extern int WPA(int ID, string sPassword, string sAxes, uint[] iParameterArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_RPA")]           public static extern int RPA(int ID, string sAxes, int uiParameterArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SPA_String")]    public static extern int SPA_String(int ID, string sAxes, uint[] iParameterArray, string sStrings);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSPA_String")]   public static extern int qSPA_String(int ID, string sAxes, uint[] iParameterArray, StringBuilder sStrings, int iMaxNameSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SEP_String")]    public static extern int SEP_String(int ID, string sPassword, string sAxes, uint[] iParameterArray, string sStrings);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSEP_String")]   public static extern int qSEP_String(int ID, string sAxes, uint[] iParameterArray, StringBuilder sStrings, int iMaxNameSize);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_STE")]            public static extern int STE(int ID, string sAxes, double[] dOffsetArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSTE")]           public static extern int qSTE(int ID, string sAxes, double[] pdValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_IMP")]            public static extern int IMP(int ID, string sAxes, double[] dImpulseSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_IMP_PulseWidth")] public static extern int IMP_PulseWidth(int ID, char cAxis, double dOffset, int iPulseWidth);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qIMP")]           public static extern int qIMP(int ID, string sAxes, double[] dValueArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SAI")]           public static extern int SAI(int ID, string sOldAxes, string sNewAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSAI")]          public static extern int qSAI(int ID, StringBuilder sAxes, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSAI_ALL")]      public static extern int qSAI_ALL(int ID, StringBuilder sAxes, int iBufferSize);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_CCL")]           public static extern int CCL(int ID, int iComandLevel, string sPassWord);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qCCL")]          public static extern int qCCL(int ID, ref int iComandLevel);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_AVG")]           public static extern int AVG(int ID, int iAverrageTime);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qAVG")]          public static extern int qAVG(int ID, ref int iAverrageTime);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qHAR")]          public static extern int qHAR(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qLIM")]          public static extern int qLIM(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTRS")]          public static extern int qTRS(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_FNL")]           public static extern int FNL(int ID, string sAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_FPL")]           public static extern int FPL(int ID, string sAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_FRF")]           public static extern int FRF(int ID, string sAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_FED")]           public static extern int FED(int ID, string sAxes, int[] iEdgeArray, int[] piParamArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qFRF")]          public static extern int qFRF(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DIO")]           public static extern int DIO(int ID, int[] iChannelsArray, int[] bValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDIO")]          public static extern int qDIO(int ID, int[] iChannelsArray, int[] bValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTIO")]          public static extern int qTIO(int ID, ref int iInputNr, ref int iOutputNr);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_IsControllerReady")] public static extern int IsControllerReady(int ID, ref int iControllerReady);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSRG")]          public static extern int qSRG(int ID, string sAxes, int[] iRegisterArray, int[] iValArray);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ATC")]           public static extern int ATC(int ID, int[] iChannels, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qATC")]          public static extern int qATC(int ID, int[] iChannels, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qATS")]          public static extern int qATS(int ID, int[] iChannels, int[] iOptions, int[] iValueArray, int iArraySize);

        
        
        /////////////////////////////////////////////////////////////////////////////
        // Macro commande
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_IsRunningMacro")] public static extern int IsRunningMacro(int ID, int[] bRunningMacro);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MAC_BEG")]       public static extern int MAC_BEG(int ID, string sMacroName);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MAC_START")]     public static extern int MAC_START(int ID, string sMacroName);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MAC_NSTART")]    public static extern int MAC_NSTART(int ID, string sMacroName, int nrRuns);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MAC_END")]       public static extern int MAC_END(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MAC_DEL")]       public static extern int MAC_DEL(int ID, string sMacroName);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MAC_DEF")]       public static extern int MAC_DEF(int ID, string sMacroName);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MAC_qDEF")]      public static extern int MAC_qDEF(int ID, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qMAC")]          public static extern int qMAC(int ID, string sMacroName, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qRMC")]          public static extern int qRMC(int ID, StringBuilder sBuffer, int iBufferSize);

        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DEL")]           public static extern int DEL(int ID, int nMilliSeconds);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WAC")]           public static extern int WAC(int ID, string sCondition);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_MEX")]           public static extern int MEX(int ID, string sCondition);


        /////////////////////////////////////////////////////////////////////////////
        // String commands.
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GcsCommandset")]    public static extern int GcsCommandset(int ID, string sCommand);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GcsGetAnswer")]     public static extern int GcsGetAnswer(int ID, StringBuilder sAnswer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GcsGetAnswerSize")] public static extern int GcsGetAnswerSize(int ID, ref int iAnswerSize);


        /////////////////////////////////////////////////////////////////////////////
        // limits.
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTMN")]          public static extern int qTMN(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTMX")]          public static extern int qTMX(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_NLM")]           public static extern int NLM(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qNLM")]          public static extern int qNLM(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_PLM")]           public static extern int PLM(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qPLM")]          public static extern int qPLM(int ID, string sAxes, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SSL")]           public static extern int SSL(int ID, string sAxes, int[] bValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSSL")]          public static extern int qSSL(int ID, string sAxes, int[] bValueArray);


        /////////////////////////////////////////////////////////////////////////////
        // Wave commands.
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_IsGeneratorRunning")] public static extern int IsGeneratorRunning(int ID, int[] iWaveGeneratorIds, int[] bValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTWG")]          public static extern int qTWG(int ID, ref int iWaveGenerators);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WAV_SIN_P")]     public static extern int WAV_SIN_P(int ID, int iWaveTableId, int iOffsetOfFirstPointInWaveTable, int iNumberOfPoints, int iAddAppendWave, int iCenterPointOfWave, double dAmplitudeOfWave, double dOffsetOfWave, int iSegmentLength);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WAV_LIN")]       public static extern int WAV_LIN(int ID, int iWaveTableId, int iOffsetOfFirstPointInWaveTable, int iNumberOfPoints, int iAddAppendWave, int iNumberOfSpeedUpDownPointsInWave, double dAmplitudeOfWave, double dOffsetOfWave, int iSegmentLength);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WAV_RAMP")]      public static extern int WAV_RAMP(int ID, int iWaveTableId, int iOffsetOfFirstPointInWaveTable, int iNumberOfPoints, int iAddAppendWave, int iCenterPointOfWave, int iNumberOfSpeedUpDownPointsInWave, double dAmplitudeOfWave, double dOffsetOfWave, int iSegmentLength);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WAV_PNT")]       public static extern int WAV_PNT(int ID, int iWaveTableId, int iOffsetOfFirstPointInWaveTable, int iNumberOfPoints, int iAddAppendWave, double[] dWavePoints);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qWAV")]          public static extern int qWAV(int ID, int[] iWaveTableIdsArray, int[] iParamereIdsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WGO")]           public static extern int WGO(int ID, int[] iWaveGeneratorIdsArray, int[] iStartModArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qWGO")]          public static extern int qWGO(int ID, int[] iWaveGeneratorIdsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WGC")]           public static extern int WGC(int ID, int[] iWaveGeneratorIdsArray, int[] iNumberOfCyclesArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qWGC")]          public static extern int qWGC(int ID, int[] iWaveGeneratorIdsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WSL")]           public static extern int WSL(int ID, int[] iWaveGeneratorIdsArray, int[] iWaveTableIdsArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qWSL")]          public static extern int qWSL(int ID, int[] iWaveGeneratorIdsArray, int[] iWaveTableIdsArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DTC")]           public static extern int DTC(int ID, int[] iDdlTableIdsArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDTL")]          public static extern int qDTL(int ID, int[] iDdlTableIdsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WCL")]           public static extern int WCL(int ID, int[] iWaveTableIdsArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTLT")]          public static extern int qTLT(int ID, int[] iNumberOfDdlTables);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qGWD_SYNC")]     public static extern int qGWD_SYNC(int ID, int iWaveTableId, int iOffsetOfFirstPointInWaveTable, int iNumberOfValues, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qGWD")]          public static extern int qGWD(int ID, int[] iWaveTableIdsArray, int iNumberOfWaveTables, int iOffset, int nrValues, ref IntPtr dValarray, StringBuilder sGcsArrayHeader, int iGcsArrayHeaderMaxSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WOS")]           public static extern int WOS(int ID, int[] iWaveTableIdsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qWOS")]          public static extern int qWOS(int ID, int[] iWaveTableIdsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WTR")]           public static extern int WTR(int ID, int[] iWaveGeneratorIdsArray, int[] iTableRateArray, int[] iInterpolationTypeArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qWTR")]          public static extern int qWTR(int ID, int[] iWaveGeneratorIdsArray, int[] iTableRateArray, int[] iInterpolationTypeArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DDL")]           public static extern int DDL(int ID, int iDdlTableId,  int iOffsetOfFirstPointInDdlTable,  int iNumberOfValues, double[] pdValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDDL_SYNC")]     public static extern int qDDL_SYNC(int ID,  int iDdlTableId,  int iOffsetOfFirstPointInDdlTable,  int iNumberOfValues, double[] pdValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDDL")]          public static extern int qDDL(int ID, int[] iDdlTableIdsArray, int iNumberOfDdlTables, int iOffset, int nrValues, ref IntPtr  dValarray, StringBuilder szGcsArrayHeader, int iGcsArrayHeaderMaxSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DPO")]           public static extern int DPO(int ID, string sAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qWMS")]          public static extern int qWMS(int ID, int[] iWaveTableIds, int[] iWaveTableMaimumSize, int iArraySize);



        /////////////////////////////////////////////////////////////////////////////
        // Trigger commands.
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_TWC")]           public static extern int TWC(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_TWS")]           public static extern int TWS(int ID, int[] iTriggerChannelIdsArray, int[] piPointNumberArray, int[] piSwitchArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTWS")]          public static extern int qTWS(int ID, int[] iTriggerChannelIdsArray, int iNumberOfTriggerChannels, int iOffset, int nrValues, ref IntPtr dValarray, StringBuilder szGcsArrayHeader, int iGcsArrayHeaderMaxSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_CTO")]           public static extern int CTO(int ID, int[] iTriggerOutputIdsArray, int[] iTriggerParameterArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qCTO")]          public static extern int qCTO(int ID, int[] iTriggerOutputIdsArray, int[] iTriggerParameterArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_TRO")]           public static extern int TRO(int ID, long[] iTriggerChannelIds, int[] bTriggerChannelEnabel, long iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTRO")]          public static extern int qTRO(int ID, long[] iTriggerChannelIds, int[] bTriggerChannelEnabel, long iArraySize);


        /////////////////////////////////////////////////////////////////////////////
        // Record tabel commands.
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qHDR")]          public static extern int qHDR(int ID, StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTNR")]          public static extern int qTNR(int ID, ref int iNumberOfRecordCannels);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DRC")]           public static extern int DRC(int ID, int[] iRecordTableIdsArray, string sRecordSourceIds, int[] iRecordOptionArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDRC")]          public static extern int qDRC(int ID, int[] iRecordTableIdsArray, StringBuilder szRecordSourceIds, int[] iRecordOptionArray, int iRecordSourceIdsBufferSize, int iRecordOptionArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDRR_SYNC")]     public static extern int qDRR_SYNC(int ID,  int iRecordTablelId,  int iOffsetOfFirstPointInRecordTable,  int iNumberOfValues, double[] dValueArray);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDRR")]          public static extern int qDRR(int ID, int[] iRecTableIdsArray, int iNumberOfRecChannels, int iOffsetOfFirstPointInRecordTable, int iNumberOfValues, ref IntPtr dValueArray, StringBuilder sGcsArrayHeader, int iGcsArrayHeaderMaxSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_DRT")]           public static extern int DRT(int ID, int[] iRecordChannelIdsArray, int[] iTriggerSourceArray, string sValues, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qDRT")]          public static extern int qDRT(int ID, int[] iRecordChannelIdsArray, int[] iTriggerSourceArray, StringBuilder sValues, int iArraySize, int iValueBufferLength);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_RTR")]           public static extern int RTR(int ID, int iReportTableRate);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qRTR")]          public static extern int qRTR(int ID, ref int iReportTableRate);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_WGR")]           public static extern int WGR(int ID);


        /////////////////////////////////////////////////////////////////////////////
        // Piezo-Channel commands.
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_VMA")]           public static extern int VMA(int ID, int[] iPiezoChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qVMA")]          public static extern int qVMA(int ID, int[] iPiezoChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_VMI")]           public static extern int VMI(int ID, int[] iPiezoChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qVMI")]          public static extern int qVMI(int ID, int[] iPiezoChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_VOL")]           public static extern int VOL(int ID, int[] iPiezoChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qVOL")]          public static extern int qVOL(int ID, int[] iPiezoChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTPC")]          public static extern int qTPC(int ID, ref int iNumberOfPiezoChannels);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_ONL")]           public static extern int ONL(int ID, int[] iPiezoChannelsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qONL")]          public static extern int qONL(int ID, int[] iPiezoChannelsArray, int[] iValueArray, int iArraySize);


        /////////////////////////////////////////////////////////////////////////////
        // Sensor-Channel commands.
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTAD")]          public static extern int qTAD(int ID, int[] iSensorsChannelsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTNS")]          public static extern int qTNS(int ID, int[] iSensorsChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTSP")]          public static extern int qTSP(int ID, int[] iSensorsChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SCN")]           public static extern int SCN(int ID, int[] iSensorsChannelsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSCN")]          public static extern int qSCN(int ID, int[] iSensorsChannelsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTSC")]          public static extern int qTSC(int ID, ref int iNumberOfSensorChannels);


        /////////////////////////////////////////////////////////////////////////////
        // PIEZOWALK(R)-Channel commands.
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_APG")]           public static extern int APG(int ID, int[] iPIEZOWALKChannelsArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSPG")]          public static extern int qAPG(int ID, int[] iPIEZOWALKChannelsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_OAD")]           public static extern int OAD(int ID, int[] iPIEZOWALKChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qOAD")]          public static extern int qOAD(int ID, int[] iPIEZOWALKChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_OCD")]           public static extern int OCD(int ID, int[] iPIEZOWALKChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qOCD")]          public static extern int qOCD(int ID, int[] iPIEZOWALKChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_OSM")]           public static extern int OSM(int ID, int[] iPIEZOWALKChannelsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qOSM")]          public static extern int qOSM(int ID, int[] iPIEZOWALKChannelsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_OVL")]           public static extern int OVL(int ID, int[] iPIEZOWALKChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qOVL")]          public static extern int qOVL(int ID, int[] iPIEZOWALKChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qOSN")]          public static extern int qOSN(int ID, int[] iPIEZOWALKChannelsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_SSA")]           public static extern int SSA(int ID, int[] iPIEZOWALKChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qSSA")]          public static extern int qSSA(int ID, int[] iPIEZOWALKChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_RNP")]           public static extern int RNP(int ID, int[] iPIEZOWALKChannelsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_PGS")]           public static extern int PGS(int ID, int[] iPIEZOWALKChannelsArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTAC")]          public static extern int qTAC(int ID, ref int nNrChannels);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qTAV")]          public static extern int qTAV(int ID, int[] iChannelsArray, double[] dValueArray, int iArraySize);


        /////////////////////////////////////////////////////////////////////////////
        // Joystick
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qJAS")]          public static extern int qJAS(int ID, int[] iJoystickIDsArray, int[] iAxesIDsArray, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_JAX")]           public static extern int JAX(int ID,  int iJoystickID,  int iAxesID, string sAxesBuffer);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qJAX")]          public static extern int qJAX(int ID, int[] iJoystickIDsArray, int[] iAxesIDsArray, int iArraySize, StringBuilder sAxesBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qJBS")]          public static extern int qJBS(int ID, int[] iJoystickIDsArray, int[] iButtonIDsArray, int[] bValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_JDT")]           public static extern int JDT(int ID, int[] iJoystickIDsArray, int[] iAxisIDsArray, int[] iValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_JLT")]           public static extern int JLT(int ID, int iJoystickID, int iAxisID, int iStartAdress, double[] dValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qJLT")]          public static extern int qJLT(int ID, int[] iJoystickIDsArray, int[] iAxisIDsArray,  int iNumberOfTables,  int iOffsetOfFirstPointInTable, int iNumberOfValues, ref IntPtr dValueArray, StringBuilder sGcsArrayHeader, int iGcsArrayHeaderMaxSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_JON")]           public static extern int JON(int ID, int[] iJoystickIDsArray, int[] bValueArray, int iArraySize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_qJON")]          public static extern int qJON(int ID, int[] iJoystickIDsArray, int[] bValueArray, int iArraySize);




        /////////////////////////////////////////////////////////////////////////////
        // Spezial
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GetSupportedFunctions")]     public static extern int GetSupportedFunctions(int ID, int[] iComandLevelArray, int iBufferSize, StringBuilder sFunctionNames, int iMaxFunctioNamesLength);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GetSupportedParameters")]    public static extern int GetSupportedParameters(int ID, int[] iParameterIdArray, int[] iComandLevelArray, int[] iMennoryLocationArray, int[] iDataTypeArray, int[] iNumberOfItems, int iBufferSize, StringBuilder sParameterName, int iMaxParameterNameSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GetSupportedControllers")]   public static extern int GetSupportedControllers(StringBuilder sBuffer, int iBufferSize);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GetAsyncBufferIndex")]       public static extern int GetAsyncBufferIndex(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_GetAsyncBuffer")]            public static extern int GetAsyncBuffer(int ID, ref IntPtr pdValueArray);


        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_AddStage")]                  public static extern int AddStage(int ID, string sAxes);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_RemoveStage")]               public static extern int RemoveStage(int ID, string sStageName);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_OpenUserStagesEditDialog")]  public static extern int OpenUserStagesEditDialog(int ID);
        [DllImport("E7XX_GCS2_DLL.dll", EntryPoint = "E7XX_OpenPiStagesEditDialog")]    public static extern int OpenPiStagesEditDialog(int ID);


	}
}
