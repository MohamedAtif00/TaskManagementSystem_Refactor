/** @type {import('next').NextConfig} */
const nextConfig = {
    reactStrictMode: true,
    swcMinify: true,
    generateEtags: false,
     typescript: {
    ignoreBuildErrors: true,
  },
};
module.exports = nextConfig;
