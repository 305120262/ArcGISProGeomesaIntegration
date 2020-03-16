package org.sean.com;

import java.io.IOException;
import java.util.*;
import java.lang.String;


import org.geotools.data.*;
import org.geotools.factory.CommonFactoryFinder;
import org.geotools.filter.text.cql2.CQLException;
import org.geotools.filter.text.ecql.ECQL;
import org.opengis.feature.simple.SimpleFeature;
import org.opengis.feature.simple.SimpleFeatureType;
import org.opengis.feature.type.AttributeDescriptor;
import org.opengis.feature.type.AttributeType;
import org.opengis.filter.Filter;
import org.opengis.filter.FilterFactory;
import org.opengis.filter.FilterFactory2;
import org.opengis.filter.identity.FeatureId;


public class GeomesaSimple {
    private DataStore datastore;

    public GeomesaSimple() throws IOException {

    }

    public static void main(String[] args) throws IOException, CQLException {
        GeomesaSimple demo = new GeomesaSimple();
        demo.connectDataStore("ubuntu.sean.com:2181");
        List<String> fields = demo.getFields("gdelt-quickstart");
        List<String> oids = demo.squeryRows("gdelt-quickstart", -120, 30, -75, 55);
        String feature = demo.findRow("gdelt-quickstart", oids.get(0));
    }

    public void connectDataStore(String endpoint) throws IOException {
        System.out.println("Loading datastore");
        Map<String, String> params = new HashMap<String, String>();
        params.put("hbase.catalog", "test");
        params.put("hbase.zookeepers", endpoint);
        datastore = DataStoreFinder.getDataStore(params);
        //datastore = new HBaseDataStoreFactory().createDataStore(params);
    }

    public  List<String> getFields(String tableName) throws IOException {
        List<String> fields = new ArrayList<String>();
        SimpleFeatureType sft = datastore.getSchema(tableName);
        for(AttributeDescriptor desc:sft.getAttributeDescriptors()){
            fields.add(desc.getLocalName());
        }
        return fields;
    }


    public List<String> squeryRows(String tableName, double xmin, double ymin, double xmax, double ymax) throws IOException, CQLException {
        // Get a FilterFactory2 to build up our query
        StringBuilder builder = new StringBuilder();
        builder.append("bbox(geom,").append(xmin).append(",").append(ymin).append(",").append(xmax).append(",").append(ymax).append(")");
        Query query = new Query(tableName, ECQL.toFilter(builder.toString()));
        List<String> oids =  new ArrayList<String>();
        try (FeatureReader<SimpleFeatureType, SimpleFeature> reader =

                     datastore.getFeatureReader(query, Transaction.AUTO_COMMIT)) {
            // loop through all results, only print out the first 10
            int n = 0;
            while (reader.hasNext()) {
                SimpleFeature feature = reader.next();
                /*
                if (n++ < 10) {
                    // use geotools data utilities to get a printable string
                    System.out.println(String.format("%02d", n) + " " + DataUtilities.encodeFeature(feature));
                } else if (n == 10) {
                    System.out.println("...");
                }
                 */
                oids.add(feature.getID());
            }
            return oids;
        }
    }

    public List<String> queryRows(String tableName) throws IOException, CQLException {
        // Get a FilterFactory2 to build up our query
        Query query = new Query(tableName, ECQL.toFilter("1=1"));
        List<String> oids =  new ArrayList<String>();
        try (FeatureReader<SimpleFeatureType, SimpleFeature> reader =

                     datastore.getFeatureReader(query, Transaction.AUTO_COMMIT)) {
            // loop through all results, only print out the first 10
            int n = 0;
            while (reader.hasNext()) {
                SimpleFeature feature = reader.next();
                /*
                if (n++ < 10) {
                    // use geotools data utilities to get a printable string
                    System.out.println(String.format("%02d", n) + " " + DataUtilities.encodeFeature(feature));
                } else if (n == 10) {
                    System.out.println("...");
                }
                 */
                oids.add(feature.getID());
            }
            return oids;
        }
    }

    public String findRow(String tableName, String oid) throws IOException, CQLException {
        FilterFactory ff = CommonFactoryFinder.getFilterFactory();
        FeatureId id= ff.featureId(tableName);

        Query query = new Query(tableName,ECQL.toFilter("IN ("+oid+")"));

        try (FeatureReader<SimpleFeatureType, SimpleFeature> reader = datastore.getFeatureReader(query, Transaction.AUTO_COMMIT)) {
            if (reader.hasNext()) {
                SimpleFeature feature = reader.next();
                String row = DataUtilities.encodeFeature(feature);
                //System.out.println(row);
                return row;
            }
            return null;
        }
    }


    public void Close() {
        datastore.dispose();
    }
}
