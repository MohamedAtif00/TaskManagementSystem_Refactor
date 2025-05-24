import { useState } from "react";
import { FiDownload, FiX } from "react-icons/fi";
import {
  FaFileImage,
  FaFilePdf,
  FaFileWord,
  FaFileExcel,
  FaFileVideo,
  FaFileAudio,
} from "react-icons/fa";

interface Attachment {
  id: number;
  type: "image" | "pdf" | "doc" | "video" | "excel" | "audio";
  url: string;
  name: string;
}

const getFileIcon = (type: Attachment["type"]) => {
  switch (type) {
    case "image":
      return <FaFileImage size={48} />;
    case "pdf":
      return <FaFilePdf size={48} />;
    case "doc":
      return <FaFileWord size={48} />;
    case "excel":
      return <FaFileExcel size={48} />;
    case "video":
      return <FaFileVideo size={48} />;
    case "audio":
      return <FaFileAudio size={48} />;
    default:
      return <FaFilePdf size={48} />;
  }
};

const AttachmentViewer = () => {
  const [selectedFile, setSelectedFile] = useState<Attachment | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const attachments: Attachment[] = [
    {
      id: 2,
      type: "pdf",
      url: "#",
      name: "document.pdf",
    },
    {
      id: 3,
      type: "image",
      url: "https://via.placeholder.com/300",
      name: "example-image.jpg",
    },
  ];

  const handleThumbnailClick = (file: Attachment) => {
    setSelectedFile(file);
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setSelectedFile(null);
  };

  const handleDownload = (
    e: React.MouseEvent<HTMLButtonElement>,
    file: Attachment
  ) => {
    e.stopPropagation();
    // Implement actual download logic
    console.log("Downloading:", file.name);
  };

  return (
    <div className="container mx-auto p-6">
      <h2 className="text-2xl font-bold mb-6">Attachments</h2>

      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        {attachments.map((file) => (
          <div
            key={file.id}
            onClick={() => handleThumbnailClick(file)}
            className="relative group cursor-pointer rounded-lg overflow-hidden shadow-md hover:shadow-lg transition-shadow duration-300"
            role="button"
            tabIndex={0}
            aria-label={`View ${file.name}`}
          >
            {file.type === "image" || file.type === "video" ? (
              <div className="aspect-square">
                <img
                  src={file.url}
                  alt={file.name}
                  className="w-full h-full object-cover"
                  loading="lazy"
                  onError={(e) => {
                    (e.target as HTMLImageElement).src =
                      "https://images.unsplash.com/photo-1682687220063-4742bd7c8012";
                  }}
                />
                {file.type === "video" && (
                  <div className="absolute inset-0 bg-black bg-opacity-40 flex items-center justify-center">
                    <div className="w-12 h-12 rounded-full bg-white flex items-center justify-center">
                      <div className="w-0 h-0 border-l-8 border-l-black border-y-6 border-y-transparent ml-1" />
                    </div>
                  </div>
                )}
              </div>
            ) : (
              <div className="aspect-square bg-gray-100 flex items-center justify-center">
                {getFileIcon(file.type)}
              </div>
            )}
            <div className="absolute bottom-0 left-0 right-0 p-2 bg-black bg-opacity-50 text-white text-sm truncate">
              {file.name}
            </div>
          </div>
        ))}
      </div>

      {isModalOpen && selectedFile && (
        <div
          className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50"
          onClick={closeModal}
        >
          <div
            className="relative max-w-4xl w-full mx-4 bg-white rounded-lg overflow-hidden"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="absolute top-4 right-4 z-10 flex gap-2">
              <button
                onClick={(e) => handleDownload(e, selectedFile)}
                className="p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
                aria-label="Download file"
              >
                <FiDownload className="w-6 h-6" />
              </button>
              <button
                onClick={closeModal}
                className="p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
                aria-label="Close modal"
              >
                <FiX className="w-6 h-6" />
              </button>
            </div>
            {selectedFile.type === "image" ? (
              <img
                src={selectedFile.url}
                alt={selectedFile.name}
                className="w-full h-auto max-h-[80vh] object-contain"
                onError={(e) => {
                  (e.target as HTMLImageElement).src =
                    "https://images.unsplash.com/photo-1682687220063-4742bd7c8012";
                }}
              />
            ) : (
              <div className="h-[80vh] flex items-center justify-center bg-gray-100">
                <div className="text-center">
                  {getFileIcon(selectedFile.type)}
                  <p className="mt-4 text-lg font-medium">{selectedFile.name}</p>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default AttachmentViewer;
