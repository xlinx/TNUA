from flask import Flask,Response,request
from flask_ngrok import run_with_ngrok

##########----------DATABASE-----------############

import pymongo

# CONNECTION_STRING = "mongodb+srv://root:(root)@cluster0-shard-00-01.qb9hj.mongodb.net/myFirstDatabase"
# client = pymongo.MongoClient(CONNECTION_STRING)

client = pymongo.MongoClient("mongodb+srv://root:decade29133824@cluster0.qb9hj.mongodb.net/mydb?retryWrites=true&w=majority")
# db = client.test
mydb = client["mydb"]

##########-----------------------------############

app = Flask(__name__)
run_with_ngrok(app)

@app.route('/webhook', methods = ["GET", "POST"])
def webhook():
    req = request.get_json(force = True)
    session = req["session"]
    query = req["queryResult"]["queryText"]
    result = req["queryResult"]["fulfillmentText"]

######### Insertin in Datdabase ###########

    data={"Query":query,
        "Result":result}
    mycol = mydb[session]
    mycol.insert_one(data)

###########################################
    return Response(status = 200)
if __name__ == '__main__':
    app.run()