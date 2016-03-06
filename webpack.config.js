var path = require('path');
var HtmlwebpackPlugin = require('html-webpack-plugin');
var webpack = require('webpack');
var merge = require('webpack-merge');

const TARGET = process.env.npm_lifecycle_event;
process.env.BABEL_ENV = TARGET;
const ENV = process.env.ENV;
const PATHS = {
  app: './js-app/js',
  build: path.join(__dirname, 'static')
};

var common = {
  entry: PATHS.app,
  output: {
    path: PATHS.build,
    filename: 'bundle.js'
  },
  resolve: {
    extensions: ['', '.js', '.jsx']
  },
  module: {
    loaders: [
      {
        test: /\.css$/,
        loaders: ['style', 'css']
      },
      {
        test: /\.jsx?$/,
        loaders: ['babel']
      }
    ]
  },
  plugins: [
    new HtmlwebpackPlugin({
      title: 'Mana Curve'
    })
  ]
};

if (process.env.ENV !== 'production') {
  module.exports = merge(common, {
    devtool: 'eval-source-map',
    devServer: {
      proxy: {
        '/deck*': {
          target: 'http://localhost:3000',
          secure: false
        }
      },
      historyApiFallback: true,
      hot: true,
      inline: true,
      progress: true,

      // display only errors to reduce the amount of output
      stats: 'errors-only',

      // parse host and port from env so this is easy
      // to customize
      host: process.env.HOST,
      port: process.env.PORT
    },
    plugins: [
      new webpack.HotModuleReplacementPlugin()
    ],
  });
} else {
  module.exports = merge(common, {
    plugins: [
      new webpack.optimize.UglifyJsPlugin()
    ]
  });
}
