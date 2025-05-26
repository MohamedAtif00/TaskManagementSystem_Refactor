// import { useState } from "react";
// import { FiDownload, FiX } from "react-icons/fi";
// import {
//   FaFileImage,
//   FaFilePdf,
//   FaFileWord,
//   FaFileExcel,
//   FaFileVideo,
//   FaFileAudio,
// } from "react-icons/fa";

// interface Attachment {
//   id: number;
//   type: "image" | "pdf" | "doc" | "video" | "excel" | "audio";
//   url: string;
//   name: string;
// }

// const getFileIcon = (type: Attachment["type"]) => {
//   switch (type) {
//     case "image":
//       return <FaFileImage size={48} />;
//     case "pdf":
//       return <FaFilePdf size={48} />;
//     case "doc":
//       return <FaFileWord size={48} />;
//     case "excel":
//       return <FaFileExcel size={48} />;
//     case "video":
//       return <FaFileVideo size={48} />;
//     case "audio":
//       return <FaFileAudio size={48} />;
//     default:
//       return <FaFilePdf size={48} />;
//   }
// };

// const AttachmentViewer = () => {
//   const [selectedFile, setSelectedFile] = useState<Attachment | null>(null);
//   const [isModalOpen, setIsModalOpen] = useState(false);

//   const attachments: Attachment[] = [
//     {
//       id: 2,
//       type: "pdf",
//       url: "#",
//       name: "document.pdf",
//     },
//     {
//       id: 3,
//       type: "image",
//       url: "https://via.placeholder.com/300",
//       name: "example-image.jpg",
//     },
//   ];

//   const handleThumbnailClick = (file: Attachment) => {
//     setSelectedFile(file);
//     setIsModalOpen(true);
//   };

//   const closeModal = () => {
//     setIsModalOpen(false);
//     setSelectedFile(null);
//   };

//   const handleDownload = (
//     e: React.MouseEvent<HTMLButtonElement>,
//     file: Attachment
//   ) => {
//     e.stopPropagation();
//     // Implement actual download logic
//     console.log("Downloading:", file.name);
//   };

//   return (
//     <div className="container mx-auto p-6">
//       <h2 className="text-2xl font-bold mb-6">Attachments</h2>

//       <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
//         {attachments.map((file) => (
//           <div
//             key={file.id}
//             onClick={() => handleThumbnailClick(file)}
//             className="relative group cursor-pointer rounded-lg overflow-hidden shadow-md hover:shadow-lg transition-shadow duration-300"
//             role="button"
//             tabIndex={0}
//             aria-label={`View ${file.name}`}
//           >
//             {file.type === "image" || file.type === "video" ? (
//               <div className="aspect-square">
//                 <img
//                   src={file.url}
//                   alt={file.name}
//                   className="w-full h-full object-cover"
//                   loading="lazy"
//                   onError={(e) => {
//                     (e.target as HTMLImageElement).src =
//                       "https://images.unsplash.com/photo-1682687220063-4742bd7c8012";
//                   }}
//                 />
//                 {file.type === "video" && (
//                   <div className="absolute inset-0 bg-black bg-opacity-40 flex items-center justify-center">
//                     <div className="w-12 h-12 rounded-full bg-white flex items-center justify-center">
//                       <div className="w-0 h-0 border-l-8 border-l-black border-y-6 border-y-transparent ml-1" />
//                     </div>
//                   </div>
//                 )}
//               </div>
//             ) : (
//               <div className="aspect-square bg-gray-100 flex items-center justify-center">
//                 {getFileIcon(file.type)}
//               </div>
//             )}
//             <div className="absolute bottom-0 left-0 right-0 p-2 bg-black bg-opacity-50 text-white text-sm truncate">
//               {file.name}
//             </div>
//           </div>
//         ))}
//       </div>

//       {isModalOpen && selectedFile && (
//         <div
//           className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50"
//           onClick={closeModal}
//         >
//           <div
//             className="relative max-w-4xl w-full mx-4 bg-white rounded-lg overflow-hidden"
//             onClick={(e) => e.stopPropagation()}
//           >
//             <div className="absolute top-4 right-4 z-10 flex gap-2">
//               <button
//                 onClick={(e) => handleDownload(e, selectedFile)}
//                 className="p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
//                 aria-label="Download file"
//               >
//                 <FiDownload className="w-6 h-6" />
//               </button>
//               <button
//                 onClick={closeModal}
//                 className="p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
//                 aria-label="Close modal"
//               >
//                 <FiX className="w-6 h-6" />
//               </button>
//             </div>
//             {selectedFile.type === "image" ? (
//               <img
//                 src={selectedFile.url}
//                 alt={selectedFile.name}
//                 className="w-full h-auto max-h-[80vh] object-contain"
//                 onError={(e) => {
//                   (e.target as HTMLImageElement).src =
//                     "https://images.unsplash.com/photo-1682687220063-4742bd7c8012";
//                 }}
//               />
//             ) : (
//               <div className="h-[80vh] flex items-center justify-center bg-gray-100">
//                 <div className="text-center">
//                   {getFileIcon(selectedFile.type)}
//                   <p className="mt-4 text-lg font-medium">{selectedFile.name}</p>
//                 </div>
//               </div>
//             )}
//           </div>
//         </div>
//       )}
//     </div>
//   );
// };

// export default AttachmentViewer;



import { useState } from "react";
import { FiDownload, FiX, FiUpload, FiFile } from "react-icons/fi";
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
  file?: File;
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

const getFileType = (file: File): Attachment["type"] => {
  const extension = file.name.split('.').pop()?.toLowerCase();
  const mimeType = file.type.toLowerCase();
  
  if (mimeType.startsWith('image/')) return 'image';
  if (mimeType === 'application/pdf' || extension === 'pdf') return 'pdf';
  if (mimeType.includes('word') || ['doc', 'docx'].includes(extension || '')) return 'doc';
  if (mimeType.includes('excel') || ['xls', 'xlsx'].includes(extension || '')) return 'excel';
  if (mimeType.startsWith('video/')) return 'video';
  if (mimeType.startsWith('audio/')) return 'audio';
  
  return 'pdf';
};

const FileUpload = ({ 
  onFileChange, 
  accept = ".pdf,.jpg,.jpeg,.png,.doc,.docx",
  maxSize = 10 * 1024 * 1024, // 10MB
  label = "اختر ملف"
}: {
  onFileChange: (file: File | null) => void;
  accept?: string;
  maxSize?: number;
  label?: string;
}) => {
  const [dragActive, setDragActive] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [error, setError] = useState('');

  const validateFile = (file: File): boolean => {
    setError('');

    // Check file size
    if (file.size > maxSize) {
      setError(`حجم الملف كبير جداً. الحد الأقصى ${Math.round(maxSize / (1024 * 1024))}MB`);
      return false;
    }

    // Check file type
    const allowedTypes = accept.split(',').map(type => type.trim());
    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();
    const isValidType = allowedTypes.some(type => 
      type === fileExtension || 
      (type.startsWith('.') && fileExtension === type) ||
      file.type.includes(type.replace('.', ''))
    );

    if (!isValidType) {
      setError('نوع الملف غير مدعوم');
      return false;
    }

    return true;
  };

  const handleFileSelect = (file: File) => {
    if (validateFile(file)) {
      setSelectedFile(file);
      onFileChange(file);
    }
  };

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true);
    } else if (e.type === "dragleave") {
      setDragActive(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileSelect(e.dataTransfer.files[0]);
    }
  };

  const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      handleFileSelect(e.target.files[0]);
    }
  };

  const removeFile = () => {
    setSelectedFile(null);
    setError('');
    onFileChange(null);
  };

  return (
    <div>
      {!selectedFile ? (
        <div
          className={`border-2 border-dashed rounded-lg p-6 text-center cursor-pointer transition-colors ${
            dragActive 
              ? 'border-blue-500 bg-blue-50' 
              : 'border-gray-300 hover:border-gray-400'
          }`}
          onDragEnter={handleDrag}
          onDragLeave={handleDrag}
          onDragOver={handleDrag}
          onDrop={handleDrop}
          onClick={() => document.getElementById('file-input')?.click()}
        >
          <FiUpload className="mx-auto h-12 w-12 text-gray-400" />
          <p className="mt-2 text-sm text-gray-600">
            اسحب الملف هنا أو اضغط للاختيار
          </p>
          <p className="text-xs text-gray-500 mt-1">
            الملفات المدعومة: PDF, صور, مستندات Word
          </p>
          <input
            id="file-input"
            type="file"
            className="hidden"
            accept={accept}
            onChange={handleFileInput}
          />
        </div>
      ) : (
        <div className="border rounded-lg p-4 bg-gray-50">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <FiFile className="h-8 w-8 text-gray-600" />
              <div>
                <p className="text-sm font-medium text-gray-900">{selectedFile.name}</p>
                <p className="text-xs text-gray-500">
                  {(selectedFile.size / (1024 * 1024)).toFixed(2)} MB
                </p>
              </div>
            </div>
            <button
              onClick={removeFile}
              className="text-red-500 hover:text-red-700"
              type="button"
            >
              <FiX className="h-5 w-5" />
            </button>
          </div>
        </div>
      )}
      
      {error && (
        <p className="mt-2 text-sm text-red-600">{error}</p>
      )}
    </div>
  );
};
const AttachmentViewer = ({
  attachments,
  onDownload,
}: {
  attachments: Attachment[];
  onDownload?: (attachment: Attachment) => void;
}) => {
  const [selectedFile, setSelectedFile] = useState<Attachment | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

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
    if (onDownload) {
      onDownload(file);
    } else {
      console.log("Downloading:", file.name);
    }
  };

  if (attachments.length === 0) return null;

  return (
    <div className="mt-4">
      <h3 className="text-lg font-medium mb-3">المرفقات</h3>

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
            ) : selectedFile.type === "video" ? (
              <video
                controls
                src={selectedFile.url}
                className="w-full max-h-[80vh] object-contain"
              />
            ) : (
              <div className="h-[80vh] flex items-center justify-center bg-gray-100">
                <div className="text-center">
                  {getFileIcon(selectedFile.type)}
                  <p className="mt-4 text-lg font-medium">
                    {selectedFile.name}
                  </p>
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
export type {Attachment}
