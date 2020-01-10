var REFR_RATE = 1000; // current data refresh rate, ms
var CNL_NUM = 101;    // the input channel number
var VIEW_ID = 2;      // the view that includes the input channel

// Start cyclic refresh of current data
function startRefreshingCurData() {
    getCurData(function (success) {
        if (!success) {
            console.warn("Error getting current data");
        }

        setTimeout(startRefreshingCurData, REFR_RATE);
    });
}

// Request and display current data
function getCurData(callback) {
    scada.clientAPI.getCurCnlData(CNL_NUM, function (success, cnlData) {
        if (success) {
            $("#divData1").html("Value = " + cnlData.Val + "<br />Status = " + cnlData.Stat);
            callback(true);
        } else {
            callback(false);
        }
    });
}

// Request and display hourly data
function getHourData() {
    var hourPeriod = new scada.HourPeriod();
    hourPeriod.date = new Date(); // current date
    hourPeriod.startHour = 0;
    hourPeriod.endHour = 23;

    var cnlFilter = new scada.CnlFilter();
    cnlFilter.cnlNums = [CNL_NUM];
    cnlFilter.viewIDs = [VIEW_ID];

    var selectMode = false; // true to get only existing data, otherwise get data hour by hour
    var requestDataAge = [];

    scada.clientAPI.getHourCnlData(hourPeriod, cnlFilter, selectMode, requestDataAge,
        function (success, hourCnlDataArr, dataAge) {
            if (success) {
                // display data
                var element = $("#divData2");
                element.html("");

                for (var item of hourCnlDataArr) {
                    element.append("<b>Hour = " + item.Hour + "</b><br />");

                    for (var cnlData of item.CnlDataExtArr) {
                        element.append(
                            "Channel = " + cnlData.CnlNum + ", " +
                            "Value = " + cnlData.Val + ", " +
                            "Status = " + cnlData.Stat + ", " +
                            "Text = " + cnlData.Text + ", " +
                            "TextWithUnit = " + cnlData.TextWithUnit + ", " +
                            "Color = " + cnlData.Color + "<br />");
                    }

                    element.append("<br />");
                }
            } else {
                console.warn("Error getting hourly data");
            }
        });
}

$(document).ready(function () {
    // initialize the API
    // if Ajax queue is not available, set scada.clientAPI.rootPath
    scada.clientAPI.ajaxQueue = scada.ajaxQueueLocator.getAjaxQueue();

    // prepare a web page
    $("#hdrTitle1").text("Get Current Data Example");
    $("#divDescr1").text("Cyclically request the current data of channel " + CNL_NUM);

    $("#hdrTitle2").text("Get Hourly Data Example");
    $("#divDescr2").text("Request the hourly data of channel " + CNL_NUM + " for the current date");

    // request data
    startRefreshingCurData();
    getHourData();
});
