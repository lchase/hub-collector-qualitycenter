# ToDo
* This code gets a 404 trying to submit to http://localhost:8090/api/workItem with the following body but it works from postman.

```json
{
  "data": {
    "type": "workItem",
    "id": 0,
    "attributes": {
      "dataSourceId": -1000,
      "externalId": "194489",
      "summary": "Wrong messages when trying to delete queues with attributes",
      "detail": "---------------------------------------------------------------------------------------------------- This defect detected by mike_entwistleSteps to Reproduce: Create and org and a simple phone queue.Create a Work Queues->Work Queue Attribute Definition (simple ext attrbitue is ok).on Work Queues->Work Queue Attribute page assign your new attribute to your simple queue and give the attribute a value. SaveGo to Work Queues->Settings page and try and delete the queue.Expected Results: Get message about attached attribute (or no message at all, not sure why we can't just delete the qeueue).Actual Results: Get the following message: Error Message: Failed to delete work queue because the queue is linked to a scheduling period or activity.Should be: Error Message: Failed to delete work queue because the queue is linked to a scheduling period, activity or attribute.System Information:  (Please remember to attach logs \r\n& screen shots.)----------------------------------------------------------------------------------------------------",
      "createdAt": "2017-03-15T00:00:00",
      "closedAt": "2017-04-24T00:00:00",
      "severity": "Medium",
      "status": "Closed",
      "updatedAt": "2017-04-24T00:00:00",
      "creator": "mike_entwistle",
      "assignee": "mike_entwistle"
    }
  }
}
```