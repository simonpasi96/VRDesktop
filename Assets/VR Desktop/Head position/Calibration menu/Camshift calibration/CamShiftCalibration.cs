public class CamShiftCalibration : CalibrationMenuItem {

    protected override void OnContinue()
    {
        FaceDetection.Instance.SendFaceToCamShift();
    }

}
