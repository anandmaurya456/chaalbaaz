const path = require('path');
const CopyPlugin = require('copy-webpack-plugin');

module.exports = {
  entry: {
    content: './src/content/index.ts',
    background: './src/background/index.ts',
    popup: './src/popup/index.ts',
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
    ],
  },
  resolve: {
    extensions: ['.tsx', '.ts', '.js'],
  },
  output: {
    filename: '[name].js',
    path: path.resolve(__dirname, 'dist'),
    clean: true,
  },
  plugins: [
    new CopyPlugin({
      patterns: [
        { from: 'public', to: '.' },
        { from: 'icons', to: 'icons' },
        { from: 'src/overlay/overlay.css', to: 'overlay.css' },
        { from: 'src/popup/popup.html', to: 'popup.html' },
      ],
    }),
  ],
};
