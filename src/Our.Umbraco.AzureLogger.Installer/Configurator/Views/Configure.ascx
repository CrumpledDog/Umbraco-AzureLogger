<%@ Control Language="c#" AutoEventWireup="True" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<script src="/umbraco/lib/angular/1.1.5/angular.min.js"></script>
<script src="/App_Plugins/AzureLogger/Install/Configurator/Controllers/Configure.js"></script>

<div ng-app ="AzureLoggerLoader">
    <div ng-controller="Loader">
        <div class="row">
            <div class="span1">
                <img src="/App_Plugins/AzureLogger/Install/umbraco-azure-logger-32.png"/>
            </div>
            <div><h4>Umbraco Azure Storage Table Logger</h4></div>
        </div>
        <div class="row">
            <div><hr /></div>
        </div>
        <div class="row" ng-show="!saved">
            <div>
                <fieldset>
                    <legend><h4>To complete installation, please enter the required parameters for the Azure storage provider below</h4></legend>
                    <form name="paramForm" class="form-horizontal" role="form">
                          <div class="control-group">
                            <ng-form name="form">
                            <label class="control-label" for="connectionString">ConnectionString</label>
                                <div class="controls">
                                  <input class="input-block-level"
                                       name="connectionString"
                                       type="text"
                                       ng-model="connectionString"
                                       required>

                                </div>
                                <span data-ng-show=" {{'form.connectionString'.$dirty && 'form.connectionString'.$error.required}}">Required!</span>
                            </ng-form>
                        </div>
                        <button preventDefault class="btn btn-primary" ng-disabled="paramForm.$invalid" ng-click="submitForm($event)">Save</button>
                    </form>
                </fieldset>
            </div>
        </div>
        <div class="row" ng-show="!saved">
            <div><hr /></div>
         </div>

        <div class="row">
            <div>
                <div class="alert alert-success" ng-show="saved && (status === 'Ok')">
                   The Azure Table Logger sucessfully configured, forever more logs will rain from the clouds
                </div>
                <div class="alert alert-error" ng-show="!saved && status === 'ConnectionError'">
                    <strong>Oh no</strong>, there was something wrong with your Azure connection string, please check and try again (ensure you're not using a ZRS or blobs only account)
                </div>
            </div>
        </div>

    </div>
</div>

